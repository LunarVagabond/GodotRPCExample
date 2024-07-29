# This could in theory be it's own project or even a totally different language 
# This is simply an example for reference
extends Node

var peer : ENetMultiplayerPeer = ENetMultiplayerPeer.new()
var port : int = 9091  # Port number to start the server on
var max_players : int = 100  # Maximum number of players allowed to connect

# Called when the node enters the scene tree for the first time.
func _ready():
	start_server()

# Function to start the server
func start_server():
	# Create the server with the specified port and max players
	var result : int = peer.create_server(port, max_players)
	if result == OK:
		# If server creation is successful, set the multiplayer peer to our ENet peer
		multiplayer.multiplayer_peer = peer
		print("Server started on port %d" % port)
		
		# Connect the peer_connected signal to the _peer_connected function
		peer.connect("peer_connected", Callable(self, "_peer_connected"))
		# Connect the peer_disconnected signal to the _peer_disconnected function
		peer.connect("peer_disconnected", Callable(self, "_peer_disconnected"))
	else:
		# If server creation fails, print the error
		print("Failed to start server: %s" % result)

# Function called when a peer connects to the server
func _peer_connected(id: int):
	print("Peer connected with ID: %d" % id)

# Function called when a peer disconnects from the server
func _peer_disconnected(id: int):
	print("Peer disconnected with ID: %d" % id)

@rpc("any_peer", "reliable")
func Ping():
	print("PING")
	var player_id = get_tree().get_multiplayer().get_remote_sender_id()
	rpc_id(player_id, "Pong")

# Note: Godot requires that the function definition for 'Pong' exists in the script
# to be able to handle the RPC call, even if it is not actively used.
# The 'Pong' function is a placeholder and does not perform any actions.
# (There must be a better way to do this but I don't have time for that)
@rpc("authority", "reliable") 
func Pong():
	return
