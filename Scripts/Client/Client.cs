using Godot;
using System;

public partial class Client : Node
{
	private const int SERVER_ID = 1;
	private const string CONNECTION_ADDRESS = "127.0.0.1";

	private ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
	private int port = 9091;  // Port number to connect to the server

	public override void _Ready()
	{
		StartClient();
	}

	// Function to start the client
	private void StartClient()
	{
		GD.Print("Running C# Client");
		// Create the client and connect to the server on the specified port
		var result = peer.CreateClient(CONNECTION_ADDRESS, port);
		if (result == Error.Ok)
		{
			// If client connection is successful, set the multiplayer peer to our ENet peer
			Multiplayer.MultiplayerPeer = peer;
			GD.Print($"Client connected to server on port {port}");

			// Connect the connected_to_server signal to the _ConnectedToServer function
			Multiplayer.Connect("connected_to_server", new Callable(this, nameof(ConnectedToServer)));
			// Connect the connection_failed signal to the _ConnectionFailed function
			Multiplayer.Connect("connection_failed", new Callable(this, nameof(ConnectionFailed)));
			// Connect the server_disconnected signal to the _ServerDisconnected function
			Multiplayer.Connect("server_disconnected", new Callable(this, nameof(ServerDisconnected)));
		}
		else
		{
			// If client connection fails, print the error
			GD.Print($"Failed to connect to server: {result}");
			GetTree().Quit();
		}
	}

	// Function called when the client successfully connects to the server
	private void ConnectedToServer()
	{
		int peerId = GetTree().GetMultiplayer().GetUniqueId();
		GD.Print($"Client successfully connected to the server: {peerId}");

		// Silly timer to arbitrarily start a Ping Pong example from client to server
		Timer pingTimer = new Timer();
		pingTimer.WaitTime = 10.0f;
		pingTimer.OneShot = true;
		pingTimer.Connect("timeout", new Callable(this, nameof(PingServer)));
		AddChild(pingTimer);
		pingTimer.Start();
	}

	// Function called when the client fails to connect to the server
	private void ConnectionFailed()
	{
		GD.Print("Client failed to connect to the server");
	}

	// Function called when the client is disconnected from the server
	private void ServerDisconnected()
	{
		GD.Print("Client disconnected from the server");
	}

	private void PingServer() => RpcId(SERVER_ID, nameof(Ping)); // GetTree().GetMultiplayer().GetUniqueId()) -> we can use this if we want to send the player id. But instead let the server handle pickup of the id

	[Rpc(mode: MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void Pong() => GD.Print("PONG");

	// Note: Godot requires that the function definition for 'Ping' exists in the script
	// to be able to handle the RPC call, even if it is not actively used.
	// The 'Ping' function is a placeholder and does not perform any actions.
	[Rpc(mode: MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void Ping() {}
}
