using Godot;
using System;

public partial class MultiplayerController : Control
{
  private const int DEFAULT_PORT = 6091;
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
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.

  public override void _Process(double delta) { }

  #region Signal Functions
  // NOTE: Runs on all peers when a player connects. The `id` is the id of player leaving
  public void PeerConnected(long id)
  {
    GD.Print($"Player Connected: {id}");
  }

  // NOTE: Runs on all peers when a player disconnectss. The `id` is the id of player leaving
  private void PeerDisconnected(long id)
  {
    GD.Print($"Player Disconnected {id}");
  }

  // NOTE: runs when the connection is succesfull on the client side
  private void ConnectedToServer()
  {
    GD.Print("Connected to Server");
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
  }

  public void OnJoinButtonPressed() 
  {
    peer = new ENetMultiplayerPeer();
    peer.CreateClient(DEFAULT_ADDRESS, DEFAULT_PORT);
    peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
    Multiplayer.MultiplayerPeer = peer;
    GD.Print("Joining Game...");
  }

  public void OnStartButtonPressed() { }

  #endregion
}
