extends Control

var _boot_text: RichTextLabel
#var _animation_player: AnimationPlayer

func _ready():
	print("BootLoader: _ready() called")
	
	#_boot_text = get_node("BootText")
	#_animation_player = get_node("AnimationPlayer")
	
	# Set responsive font size for boot text
	#update_boot_text_size()
	
	# Connect to viewport size changes
	#get_viewport().size_changed.connect(_on_viewport_size_changed)
	
	# Animation is handled by AnimationPlayer (autoplay)
	print("BootLoader: AnimationPlayer will handle boot sequence")
	
	var terminal_text = get_node("TerminalText")
	terminal_text.terminal_text_finished.connect(_on_terminal_text_finished)

func _on_terminal_text_finished():
	#TODO: Show Main Title and then switch to Main Terminal.
	
	#var global = get_node("/root/Global")
	#global.goto_scene("res://Scenes/MainTerminal.tscn")
	pass

#func _on_viewport_size_changed():
	#update_boot_text_size()

# Called by AnimationPlayer at appropriate time
func on_main_title_animation_completed():
	print("BootLoader: MainTitle animation completed")
	# This is now just a marker for logging, actual wait is in animation

# Called by AnimationPlayer at 8.0s
func change_scene():
	print("BootLoader: change_scene() called")
	var result = get_tree().change_scene_to_file("res://Scenes/MainTerminal.tscn")
	
	if result != OK:
		push_error("BootLoader: Scene change failed with error: %s" % result)
	else:
		print("BootLoader: Scene change initiated successfully")

#func update_boot_text_size():
	#if ResolutionManager.get_instance() != null and _boot_text != null:
		#var base_font_size = 24
		#var scaled_font_size = ResolutionManager.get_scaled_font_size(base_font_size)
		#
		#_boot_text.add_theme_font_size_override("normal_font_size", scaled_font_size)
		#
		## Also update the text area size
		#var box_width = ResolutionManager.get_percentage_of_screen_width(80.0) # 80% of screen width
		#var box_height = ResolutionManager.get_percentage_of_screen_height(15.0) # 15% of screen height
		#
		#_boot_text.offset_left = -box_width / 2
		#_boot_text.offset_right = box_width / 2
		#_boot_text.offset_top = -box_height / 2
		#_boot_text.offset_bottom = box_height / 2
		#
		#print("BootLoader: Font size set to %dpx, box size %dx%d" % [scaled_font_size, box_width, box_height])
