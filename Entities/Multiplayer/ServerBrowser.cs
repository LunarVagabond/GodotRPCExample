using Godot;
using System;
using System.Linq;
using System.Text.Json;

public partial class ServerBrowser : Control
{
  [Export] PacketPeerUdp Broadcaster;
  [Export] PacketPeerUdp Listener;

  [Export] int LisentPort = 8911;
  [Export] int HostPort = 8912;
  [Export] string BroadcastAddress = "192.168.4.38";	

  [Export] Timer BroadcastTimer;
  [Export] PackedScene ServerInfoScene;
  [Export] VBoxContainer ServerListVBox;
  [Export] Label TEMP_LISTEN_LABEL;

	[Signal] public delegate void JoinGameEventHandler(string ip);

  private ServerInfo serverInfo;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    SetupListener();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta){
    if (Listener.GetAvailablePacketCount() > 0)
    {
      string serverIP = Listener.GetPacketIP();
      int serverPort = Listener.GetPacketPort();
      byte[] serverInfoBytes = Listener.GetPacket();

      ServerInfo info = JsonSerializer.Deserialize<ServerInfo>(serverInfoBytes.GetStringFromUtf8());
      GD.Print($"ServerIP: {serverIP}, Port: {serverPort}, Server Info: {serverInfoBytes.GetStringFromAscii()}");
      
      Node currentNode = ServerListVBox.GetChildren().Where(child => child.Name == info.Name).FirstOrDefault();

      if (currentNode != null) 
      {
        currentNode.GetNode<Label>("PlayerCountLabel").Text = info.PlayerCount.ToString();;
        currentNode.GetNode<Label>("IPLabel").Text = serverIP;
        return;
      }

      ServerBrowserInfoLine serverInfo = ServerInfoScene.Instantiate<ServerBrowserInfoLine>();
      serverInfo.Name = info.Name;
      serverInfo.GetNode<Label>("NameLabel").Text = serverInfo.Name;
      serverInfo.GetNode<Label>("IPLabel").Text = serverIP;
      serverInfo.GetNode<Label>("PlayerCountLabel").Text = info.PlayerCount.ToString();
      ServerListVBox.AddChild(serverInfo);
      serverInfo.JoinGame += OnJoinGame;
    }
  }

  private void OnJoinGame(string ip)
  {
    EmitSignal(SignalName.JoinGame, ip);
  }

  public void SetupBroadcast(string name)
  {
    Broadcaster =  new PacketPeerUdp();
    serverInfo = new ServerInfo()
    { 
      Name = name, 
      PlayerCount = GameManager.Players.Count 
    };
    Broadcaster.SetBroadcastEnabled(true);
    Broadcaster.SetDestAddress(BroadcastAddress, LisentPort);
    var ok = Broadcaster.Bind(HostPort);

    if (ok == Error.Ok) { GD.Print($"Bound broadcaster to port: {HostPort}"); }
    else                { GD.Print($"Failed to bind Broadcast Port: {HostPort}"); }

    BroadcastTimer.Start();
  }

  private void SetupListener()
  {
    Listener =  new PacketPeerUdp();
    var ok = Listener.Bind(LisentPort);

    if (ok == Error.Ok) { GD.Print($"Bound listener to port: {LisentPort}"); }
    else                { GD.Print($"Failed to bind Listener Port: {LisentPort}"); }
    TEMP_LISTEN_LABEL.Text = $"Bound to Listen Port: {ok  == Error.Ok}"; // FIXME: Remove this shiz
  }

  private void OnBroadCastTimerTimeout()
  {
    GD.Print("Brodcasting Game Data...");
    serverInfo.PlayerCount = GameManager.Players.Count;

    string json = JsonSerializer.Serialize(serverInfo);
    byte[] packet = json.ToUtf8Buffer(); // Asci is only good for english and not many symbols

    Broadcaster.PutPacket(packet);
  }

  public void CleanUp()
  {
    Listener.Close();
    BroadcastTimer.Stop();

    if(Broadcaster != null)
    {
      Broadcaster.Close();
    }
  }

}
