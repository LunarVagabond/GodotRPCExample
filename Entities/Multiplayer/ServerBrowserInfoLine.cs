using Godot;
using System;

public partial class ServerBrowserInfoLine : HBoxContainer
{

	[Export] Label IPLabel;

	ServerInfo serverInfo;

	[Signal] public delegate void JoinGameEventHandler(string ip);

	private void JoinGameButtonDown()
	{
		EmitSignal(SignalName.JoinGame, IPLabel.Text);
	}
}
