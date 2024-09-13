using Godot;
using System;

public partial class Main : Node
{

	[Export] Label TempHostClientDisplayLabel;

	public override void _Ready()
	{
		GD.Print("Running C# Version");
		// Check for command-line arguments to determine run mode
		var args = OS.GetCmdlineArgs();
		GD.Print($"Command-line argument: {args}");
		bool isServer = false;
		TempHostClientDisplayLabel.Text = "Client";

		foreach (string arg in args)
		{
			if (arg == "--server")
			{
				TempHostClientDisplayLabel.Text = "SERVER";
				GD.Print("Is Server");
				isServer = true;
			}
		}

		if (isServer)
		{
			// Add the server script as a child
			var serverScript = GD.Load<CSharpScript>("res://Scripts/Server/Server.cs");
			var server = (Node)serverScript.New();
			AddChild(server);
		}
		else
		{
			// Add the client script as a singleton and as a child
			var clientScript = GD.Load<CSharpScript>("res://Scripts/Client/Client.cs");
			var client = (Node)clientScript.New();
			AddChild(client);
			// Set the path for the autoload singleton
			ProjectSettings.SetSetting("autoloads/Client/path", "res://Scripts/Singletons/Client.cs");
			// Enable the autoload singleton
			ProjectSettings.SetSetting("autoloads/Client", true);
		}
	}
}
