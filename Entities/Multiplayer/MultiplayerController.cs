using Godot;
using System;
using System.Linq;

public partial class MultiplayerController : Control
{
  [Export] private LineEdit PlayerNameNode;
  [Export] private Control ServerBrowserControl;

  private const int DEFAULT_PORT = 8910;
  private const string DEFAULT_ADDRESS = "127.0.0.1";
  private const int MAX_CLIENTS = 4;

  private ENetMultiplayerPeer peer;


  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    Multiplayer.PeerConnected += PeerConnected;
    Multiplayer.PeerDisconnected += PeerDisconnected;
    Multiplayer.ConnectedToServer += ConnectedToServer;
    Multiplayer.ConnectionFailed += ConnectionFailed;

    if (OS.GetCmdlineArgs().Contains("--server")) { HostGame(); }

    (ServerBrowserControl as ServerBrowser).JoinGame += JoinGame;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) { }

  private void HostGame()
  {
    peer = new ENetMultiplayerPeer();
    var error = peer.CreateServer(DEFAULT_PORT, MAX_CLIENTS);
    if (error != Error.Ok)
    {
      GD.PrintErr($"Hosting Failed: {error}");
      return;
    }
    peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
    Multiplayer.MultiplayerPeer = peer; // Connect the hosting player to the server as a player
    GD.Print("Waiting for Players...");

    (ServerBrowserControl as ServerBrowser).SetupBroadcast(PlayerNameNode.Text + "'s Server");
  }
  
  #region Signal Functions
  // NOTE: Runs on all peers when a player connects. The `id` is the id of player leaving
  public void PeerConnected(long id)
  {
    GD.Print($"Player Connected: {id}");
  }

  // NOTE: Runs on all peers when a player disconnectss. The `id` is the id of player leaving
  // FIXME: Never Emmitted right now
  private void PeerDisconnected(long id)
  {
    GD.Print($"Player Disconnected {id}");
    GameManager.Players.Remove(GameManager.Players.Where(i => i.Id == id).First<PlayerInfo>());
    var players = GetTree().GetNodesInGroup("Player");
    foreach (var p in players)
    {
      if (p.Name == id.ToString())
      {
        GD.Print("found it");
        p.QueueFree();
      }
    }

  }

  // NOTE: runs when the connection is succesfull on the client side
  private void ConnectedToServer()
  {
    GD.Print("Connected to Server");
    RpcId(1, "SendPlayerInfo", PlayerNameNode.Text, Multiplayer.GetUniqueId()); // Run on the server only (1 is always server id unless you dork something up)
  }

  // NOTE: Runs when connection fails from client not server
  private void ConnectionFailed()
  {
    GD.Print("Connection Failed");
    GetTree().Quit();
  }
  #endregion


  #region Button Functions

  public void OnHostButtonPressed()
  {
    HostGame();
    SendPlayerInfo(PlayerNameNode.Text, 1); // Send host info to everyone as well
  }

  public void OnJoinButtonPressed() => JoinGame(DEFAULT_ADDRESS);

  private void JoinGame(string ip)
  {
    peer = new ENetMultiplayerPeer();
    peer.CreateClient(ip, DEFAULT_PORT);
    peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
    Multiplayer.MultiplayerPeer = peer;
    GD.Print("Joining Game...");
  }

  public void OnStartButtonPressed() { Rpc("StartGame"); }
  #endregion

  #region RPC's

  [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
  private void StartGame()
  {
    (ServerBrowserControl as ServerBrowser).CleanUp();
    foreach (PlayerInfo player in GameManager.Players)
    {
      GD.Print($"Player: {player.Name}");
    }
    Node2D scene = ResourceLoader.Load<PackedScene>("res://Scenes/level_one.tscn").Instantiate<Node2D>();
    GetTree().Root.AddChild(scene);
    this.QueueFree();
  }

  [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
  private void SendPlayerInfo(string name, int id)
  {
    PlayerInfo playerInfo = new PlayerInfo() { Name = name, Id = id };
    if (!GameManager.Players.Contains(playerInfo))
    {
      GameManager.Players.Add(playerInfo);
    }

    if (Multiplayer.IsServer())
    {
      foreach (PlayerInfo player in GameManager.Players)
      {
        Rpc("SendPlayerInfo", player.Name, player.Id);
      }
    }
  }

  #endregion
}
