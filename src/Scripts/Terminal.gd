extends Control
class_name Terminal

enum MessageType {
	NORMAL,
	SYSTEM,
	ERROR,
	WARNING,
	SUCCESS
}

var _output: RichTextLabel
var _command_queue: Array = []
var _typewriter_delay = 0.03
var _typewriter_timer = 0.0
var _displayed_text = ""

# Current line being typed
var _current_formatted_text = ""
var _current_plain_text = ""
var _char_index = 0

# Progress bar flag
var _start_progress_bar_when_ready = false

# Colors for message types
var _system_color = Color(0.2, 1.0, 0.2)
var _error_color = Color(1.0, 0.2, 0.2)
var _warning_color = Color(1.0, 1.0, 0.2)

func _ready():
	_output = get_node("TerminalOutput")
	_output.clear()
	_output.bbcode_enabled = true
	
	# Set responsive font size
	update_font_size()
	
	print("Terminal: _ready() called - displaying startup sequence")
	
	# Display startup sequence
	show_startup_sequence()
	
	# Connect to viewport size changes
	get_viewport().size_changed.connect(_on_viewport_size_changed)

func _on_viewport_size_changed():
	update_font_size()

func update_font_size():
	if ResolutionManager.get_instance() != null:
		var base_font_size = 16
		var scaled_font_size = ResolutionManager.get_scaled_font_size(base_font_size)
		
		# Update the font size
		_output.add_theme_stylebox_override("normal", StyleBoxEmpty.new())
		_output.add_theme_font_size_override("normal_font_size", scaled_font_size)
		_output.add_theme_font_size_override("bold_font_size", scaled_font_size)
		_output.add_theme_font_size_override("italics_font_size", scaled_font_size)
		_output.add_theme_font_size_override("mono_font_size", scaled_font_size)
		
		# Update margins based on screen size
		var base_margin = 20.0
		var scaled_margin = ResolutionManager.get_scaled_margin(base_margin)
		
		# Apply responsive margins
		_output.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		_output.offset_left = scaled_margin
		_output.offset_top = scaled_margin + ResolutionManager.safe_area_top
		_output.offset_right = -scaled_margin
		_output.offset_bottom = -scaled_margin
		
		print("Terminal: Font size updated to %dpx, margins to %.0fpx" % [scaled_font_size, scaled_margin])

func show_startup_sequence():
	# Clear any existing content
	clear()
	
	# Display ASCII logo instantly
	#display_logo()
	
	# Then show the rest with typewriter effect
	print_line("", MessageType.NORMAL)
	print_line("GHOSTD TERMINAL v1.0.0", MessageType.SYSTEM)
	print_line("=====================================", MessageType.SYSTEM)
	print_line("", MessageType.NORMAL)
	print_line("Initializing core systems...", MessageType.SYSTEM)
	print_line("Loading AI modules...", MessageType.SYSTEM)
	print_line("", MessageType.NORMAL)
	
	# Add a placeholder line for the progress bar
	print_line("INITIALIZING [                         ] 0%", MessageType.SYSTEM)
	
	# Set a flag to start progress bar when typewriter completes
	_start_progress_bar_when_ready = true

func start_progress_bar():
	# Create and start the progress bar
	var progress_bar = preload("res://Scripts/UI/TerminalProgressBar.gd").new()
	add_child(progress_bar)
	
	progress_bar.on_complete = func():
		# Auto-transition to main menu after progress completes
		get_tree().create_timer(0.5).timeout.connect(func():
			# Find the InputArea node which has the TerminalInput script
			var terminal_input = get_node("InputArea")
			if terminal_input != null:
				terminal_input.transition_to_main_menu()
		)
		progress_bar.queue_free()
	
	# Use width of 18 for safety on small screens
	# Format: "INITIALIZING [██████████████████] 100%"
	progress_bar.start_progress(self, "INITIALIZING", 3.0, 18)

func _process(delta):
	# Handle typewriter effect
	if not _current_plain_text.is_empty():
		_typewriter_timer += delta
		
		# For very fast typewriter speeds, process multiple characters per frame
		var chars_to_process = 1
		if _typewriter_delay < 0.01: # If delay is less than 10ms
			# Calculate how many characters we should show this frame
			chars_to_process = max(1, int(delta / _typewriter_delay))
		
		while _typewriter_timer >= _typewriter_delay and _char_index < _current_plain_text.length() and chars_to_process > 0:
			_typewriter_timer -= _typewriter_delay
			_char_index += 1
			chars_to_process -= 1
			
			# Check if line is complete
			if _char_index >= _current_plain_text.length():
				# Add the complete formatted line to our history
				_displayed_text += _current_formatted_text
				
				# Reset for next line
				_current_formatted_text = ""
				_current_plain_text = ""
				_char_index = 0
				_typewriter_timer = 0.0
				
				# Process next command if available
				if _command_queue.size() > 0:
					process_next_command()
				break
		
		# Update display after processing characters
		if _char_index > 0 and not _current_plain_text.is_empty():
			update_display()
	elif _command_queue.size() > 0:
		process_next_command()
	elif _start_progress_bar_when_ready:
		# All typewriter animations complete, start progress bar
		_start_progress_bar_when_ready = false
		start_progress_bar()

func update_display():
	_output.clear()
	_output.append_text(_displayed_text)
	
	# Add the current line with typewriter effect
	if not _current_plain_text.is_empty():
		# For colored text, we need to wrap the partial text in color tags
		if _current_formatted_text.contains("[color="):
			# Extract the color tag
			var regex = RegEx.new()
			regex.compile("\\[color=(#[0-9a-fA-F]+)\\]")
			var result = regex.search(_current_formatted_text)
			if result:
				var color = result.get_string(1)
				var partial_text = _current_plain_text.substr(0, _char_index)
				_output.append_text("[color=%s]%s[/color]" % [color, partial_text])
			else:
				# Fallback to showing the whole line
				_output.append_text(_current_formatted_text)
		else:
			# For non-colored text, show character by character
			_output.append_text(_current_plain_text.substr(0, _char_index))

func print_line(text: String, type: int = MessageType.NORMAL):
	var colored_text = format_text(text, type)
	var plain_text = strip_bbcode(colored_text) + "\n"
	_command_queue.append({"formatted": colored_text + "\n", "plain": plain_text})

func print_text(text: String, type: int = MessageType.NORMAL):
	var colored_text = format_text(text, type)
	var plain_text = strip_bbcode(colored_text)
	_command_queue.append({"formatted": colored_text, "plain": plain_text})

func print_instant(text: String, type: int = MessageType.NORMAL):
	# Display text instantly without typewriter effect
	var colored_text = format_text(text, type)
	_displayed_text += colored_text + "\n"
	_output.clear()
	_output.append_text(_displayed_text)

## Updates the last line in the terminal (useful for progress bars)
func update_last_line(text: String, type: int = MessageType.NORMAL):
	# Cancel any ongoing typewriter animation
	_current_formatted_text = ""
	_current_plain_text = ""
	_char_index = 0
	_command_queue.clear()
	
	if _displayed_text.length() == 0:
		# If no text exists, just print normally
		print_instant(text, type)
		return
	
	# Find the last newline in the displayed text
	var search_start = _displayed_text.length() - 2 if _displayed_text.length() > 1 else 0
	var last_newline_index = _displayed_text.rfind("\n", search_start) if search_start > 0 else -1
	
	if last_newline_index >= 0:
		# Remove everything after the last newline
		_displayed_text = _displayed_text.substr(0, last_newline_index + 1)
		
		# Add the new line
		var colored_text = format_text(text, type)
		_displayed_text += colored_text + "\n"
		
		# Refresh display
		_output.clear()
		_output.append_text(_displayed_text)
	else:
		# If no lines exist, just print normally
		print_instant(text, type)

func clear():
	if _output != null:
		_output.clear()
	_displayed_text = ""
	_command_queue.clear()
	_current_formatted_text = ""
	_current_plain_text = ""
	_char_index = 0

func show_battle_intro(ai_name: String, enemy_name: String):
	clear()
	print_line("=== BATTLE INITIATED ===", MessageType.SYSTEM)
	print_line("", MessageType.NORMAL)
	print_line("Your AI: %s" % ai_name, MessageType.SYSTEM)
	print_line("Enemy: %s" % enemy_name, MessageType.ERROR)
	print_line("", MessageType.NORMAL)
	print_line("Analyzing combat parameters...", MessageType.NORMAL)
	print_line("Loading battle protocols...", MessageType.NORMAL)
	print_line("", MessageType.NORMAL)

func process_next_command():
	if _command_queue.size() > 0:
		var next = _command_queue.pop_front()
		_current_formatted_text = next.formatted
		_current_plain_text = next.plain
		_char_index = 0

func strip_bbcode(text: String) -> String:
	# Remove BBCode tags but keep the content
	var regex = RegEx.new()
	regex.compile("\\[.*?\\]")
	return regex.sub(text, "", true)

func format_text(text: String, type: int) -> String:
	match type:
		MessageType.SYSTEM:
			return "[color=#22ff22]%s[/color]" % text
		MessageType.ERROR:
			return "[color=#ff2222]%s[/color]" % text
		MessageType.WARNING:
			return "[color=#ffff22]%s[/color]" % text
		MessageType.SUCCESS:
			return "[color=#22ffff]%s[/color]" % text
		_:
			return text

#func display_logo():
	## ASCII art logo
	#var logo_lines = [
		#"                                          ▒░",
		#"             ░▒░                    ▒▒░░                 ░░░     ░░░",
		#"            ░░   ░░░░                      ░░   ░░░░",
		#"        ░░░              ▒ ░░        ░░░             ░▒░░░   ▒▒▒░░▒░   ░   ░",
		#"     ░░░░░░░░░ ░ ░░░░  ░░░░░░  ░░░░░░░░ ░░░ ░░░░░░░░  ░░░░░░░░░░░ ░░░░░░░░░░",
		#"  ░░ ▒███████▒░  ▓██▓░  ▒██▓░ ░▒███████▒   ░▓██████▒░ ░█████████▓ ▒████████▓░ ░",
		#"    ▒███▓▓▓▓███▒░▒██▒░  ▒██▓░ ▓███▓▓▓████ ▒███▓▓▓████░░▓▓▓████▓▓▒ ░████▓▓████▒░",
		#" ░░ ▒██▓    ███░ ▓██▓░  ▒██▓░ ▓██▒   ░███ ▒██▓░  ░██▓░    ▓██░    ░███░  ░▓██▒",
		#"    ▒██▓    ░░░░ ▓██▓░  ▒██▓░ ███░   ░███ ▒██▓░        ░░ ▓██▒  ░ ░███░   ▓██▒░░░",
		#"  ░▒▒██▓  ░░     ▓███▓▓▓███▒░ ▓██▒ ░░░███ ▒████▓▒░  ░░    ▓██▒    ░███░   ▒██▒",
		#" ░░ ▒██▓ ░▓██▓▓░ ▓█████████▓░ ▓██▒ ░ ░███  ░▓██████▓░  ░░░▓██▒ ░░ ░███░   ▓██▒",
		#"    ▒██▓ ░████▓░ ▓██▓░  ▒██▓░ ▓██▒ ░ ░██▓     ░▒▓▓███░   ░▓██▒ ░░░░███░   ▓██▒ ░░",
		#"░░░░ ▒██▓ ░░░███░ ▓██▓░  ▒██▓░ ▓██▒   ░███   ░░   ░███░    ▓██░    ░███░   ▓██▒",
		#" ░░ ▓██▓    ███░ ▓██▒░  ▒██▓░ ▓██▒   ░███ ░█▒▒░  ░███░░▒░ ▓██▒ ░  ░███░ ░░▓██▒░░",
		#"    ▒██▓░░░░███░ ▓██▓░░░▒██▓░ ▓██▒░░░▒███ ▒██▓░░░▒███░    ▓██▒  ░░░███▒░░▒███▒",
		#"    ░▓█████████░ ▓██▓░░░░██▓░ ▒█████████░ ░▓████████▒░░░  ▓██▒  ░░░█████████▒░",
		#" ░░░ ░▒▓▓▓▓▓▓▒░  ▒▓▓▒░  ░▒▓▒░  ░▒▓▓▓▓▒░    ░▒▓▓▓▓▓░   ░░ ▒▓▓░░   ░▒▓▓▓▓▓▓▓░ ░▒░",
		#"               ░░    ░  ░░░░    ░░░  ░░░ ░░    ░░░░   ░ ░░░      ░░░     ░░░",
		#"       ░░                        ░             ░░░              ░░░",
		#"            ░             ░░      ░░                                    ▒",
		#"                                         ░░░░░░░              ░░"
	#]
	#
	## Join all lines into a single string and display instantly
	#var full_logo = "\n".join(logo_lines)
	#print_instant(full_logo, MessageType.SUCCESS)
