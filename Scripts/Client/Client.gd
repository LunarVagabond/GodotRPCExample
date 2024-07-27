extends Node

const SERVER_ID = 1

var peer : ENetMultiplayerPeer = ENetMultiplayerPeer.new()
var port : int = 9091  # Port number to connect to the server

# Called when the node enters the scene tree for the first time.
func _ready():
	start_client()

# Function to start the client
func start_client():
	# Create the client and connect to the server on the specified port
	var result : int = peer.create_client("127.0.0.1", port)
	if result == OK:
		# If client connection is successful, set the multiplayer peer to our ENet peer
		multiplayer.multiplayer_peer = peer
		print("Client connected to server on port %d" % port)
		
		# Connect the connected_to_server signal to the _connected_to_server function
		multiplayer.connect("connected_to_server", Callable(self, "_connected_to_server"))
		# Connect the connection_failed signal to the _connection_failed function
		multiplayer.connect("connection_failed", Callable(self, "_connection_failed"))
		# Connect the server_disconnected signal to the _server_disconnected function
		multiplayer.connect("server_disconnected", Callable(self, "_server_disconnected"))
	else:
		# If client connection fails, print the error
		print("Failed to connect to server: %s" % result)

# Function called when the client successfully connects to the server
func _connected_to_server():
	var peer_id : int = get_tree().get_multiplayer().get_unique_id()
	print("Client successfully connected to the server: ",  peer_id)
	
	# Silly timer to arbitraily start a Ping Pong example from client to server
	var ping_timer = Timer.new()
	ping_timer.wait_time = 10.0
	ping_timer.one_shot = true
	ping_timer.connect("timeout", Callable(self, "PingServer"))
	add_child(ping_timer)
	ping_timer.start()

# Function called when the client fails to connect to the server
func _connection_failed():
	print("Client failed to connect to the server")

# Function called when the client is disconnected from the server
func _server_disconnected():
	print("Client disconnected from the server")

func PingServer():
	rpc_id(SERVER_ID, &"Ping", multiplayer.get_unique_id())

@rpc("authority", "reliable") # Only the server can call this function
func Pong():
	print("PONG")

# Note: Godot requires that the function definition for 'Ping' exists in the script
# to be able to handle the RPC call, even if it is not actively used.
# The 'Ping' function is a placeholder and does not perform any actions.
# (There must be a better way to do this but I don't have time for that)
@rpc("any_peer", "reliable")
func Ping(_player_id):
	return
