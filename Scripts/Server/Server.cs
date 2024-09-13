using Godot;
using System;

public partial class Server : Node
{
	private const string DEFAULT_ADDRESS = "127.0.0.1";  // Port number to start the server on
	private const int DEFAULT_PORT = 9091;  // Port number to start the server on

	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();

	private int maxPlayers = 100;  // Maximum number of players allowed to connect

	public string selected_address;
	public int selected_port;

	public override void _Ready()
	{
		StartServer();
	}

	// Function to start the server
	private void StartServer()
	{
		GD.Print("Running C# Server");
		// Create the server with the specified port and max players
		var result = peer.CreateServer(DEFAULT_PORT, maxPlayers);
		if (result == Error.Ok)
		{
			// If server creation is successful, set the multiplayer peer to our ENet peer
			Multiplayer.MultiplayerPeer = peer;
			GD.Print($"Server started on port {DEFAULT_PORT}");

			// Connect the peer_connected signal to the _peer_connected function
			peer.Connect("peer_connected", new Callable(this, nameof(PeerConnected)));
			// Connect the peer_disconnected signal to the _peer_disconnected function
			peer.Connect("peer_disconnected", new Callable(this, nameof(PeerDisconnected)));
		}
		else
		{
			// If server creation fails, print the error
			GD.Print($"Failed to start server: {result}");
		}
	}

	// Function called when a peer connects to the server
	private void PeerConnected(int id)
	{
		GD.Print($"Peer connected with ID: {id}");
	}

	// Function called when a peer disconnects from the server
	private void PeerDisconnected(int id)
	{
		GD.Print($"Peer disconnected with ID: {id}");
	}

	[Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void Ping()
	{
		GD.Print("PING");
		long playerId = Multiplayer.GetRemoteSenderId();
		RpcId(playerId, "Pong");
	}

	// Note: Godot requires that the function definition for 'Pong' exists in the script
	// to be able to handle the RPC call, even if it is not actively used.
	// The 'Pong' function is a placeholder and does not perform any actions.
	[Rpc(mode: MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void Pong()
	{
		// Placeholder function
	}
}
