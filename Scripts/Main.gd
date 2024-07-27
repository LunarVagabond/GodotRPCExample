extends Node

# Called when the node enters the scene tree for the first time.
func _ready():
	# Check for command-line arguments to determine mode
	var args = OS.get_cmdline_args()
	var is_server = false
	for arg in args:
		if arg == "--server":
			is_server = true

	if is_server:
		# Add the server script as a child
		var server = load("res://Scripts/Server/Server.gd").new()
		add_child(server)
	else:
		# Add the client script as a singleton and as a child
		var client = load("res://Scripts/Client/Client.gd").new()
		add_child(client)
		# Set the path for the autoload singleton
		ProjectSettings.set_setting("autoloads/Client/path", "res://Scripts/Singletons/Client.gd")
		# Enable the autoload singleton
		ProjectSettings.set_setting("autoloads/Client", true)
