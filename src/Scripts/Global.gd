extends Node

var current_scene = null

# Called when the node enters the scene tree for the first time.
func _ready():
	var root = get_tree().root
	# Using a negative index counts from the end, so this gets the last child node of root.
	current_scene = root.get_child(-1)

func goto_scene(path):
	# This function will usually be called from a signal callback,
	# or some other function from the current scene.
	# Deleting the current scene at this point is
	# a bad idea, because it may still be executing code.
	# This will result in a crash or unexpected behavior.
	
	# The solution is to defer the load to a later time, when
	# we can be sure that no code from the current scene is running:
	
	call_deferred("_deferred_goto_scene", path)

func _deferred_goto_scene(path):
	# It is now safe to remove the current scene.
	current_scene.free()
	
	# Load a new scene.
	var next_scene = load(path)
	
	# Instance the new scene.
	current_scene = next_scene.instantiate()
	
	# Add it to the active scene, as child of root.
	get_tree().root.add_child(current_scene)
	
	# Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
	get_tree().current_scene = current_scene
