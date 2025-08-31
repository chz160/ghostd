extends Control

enum GameState {
	STARTUP,
	MAIN_MENU,
	PRE_BATTLE,
	BATTLE,
	POST_BATTLE,
	MODULE_LAB,
	MODULE_LAB_MENU,
	ARCHIVES,
	SYSTEM
}

var _terminal: Terminal
var _tap_detector: Button
var _current_state = GameState.STARTUP

var _main_menu_options = [
	{"key": "1", "label": "NEW BATTLE", "target_state": GameState.PRE_BATTLE},
	{"key": "2", "label": "MODULE LAB", "target_state": GameState.MODULE_LAB},
	{"key": "3", "label": "ARCHIVES", "target_state": GameState.ARCHIVES},
	{"key": "4", "label": "SYSTEM", "target_state": GameState.SYSTEM}
]

func _ready():
	# Get the Terminal node which is a sibling of InputArea's parent
	_terminal = get_node("/root/MainTerminal/Terminal")
	_tap_detector = get_node("TapDetector")
	
	if _terminal == null:
		push_error("TerminalInput: Failed to find Terminal node!")
		return
	
	_tap_detector.pressed.connect(_on_tap_detected)
	
	# Set responsive input area height
	update_input_area_size()
	
	# Connect to viewport size changes
	get_viewport().size_changed.connect(_on_viewport_size_changed)
	
	# Don't show menu on startup - let the terminal show its startup sequence first

func _input(event):
	if event is InputEventKey and event.pressed:
		if _current_state == GameState.MAIN_MENU:
			match event.keycode:
				KEY_1:
					handle_menu_selection(0)
				KEY_2:
					handle_menu_selection(1)
				KEY_3:
					handle_menu_selection(2)
				KEY_4:
					handle_menu_selection(3)
		elif _current_state == GameState.MODULE_LAB_MENU:
			match event.keycode:
				KEY_1:
					transition_to_ai_assembly()
				KEY_2:
					transition_to_module_viewer()
				KEY_3:
					_current_state = GameState.MAIN_MENU
					show_main_menu()

func handle_menu_selection(index: int):
	if index < 0 or index >= _main_menu_options.size():
		return
	
	var option = _main_menu_options[index]
	_current_state = option.target_state
	
	match option.target_state:
		GameState.PRE_BATTLE:
			show_pre_battle()
		GameState.MODULE_LAB:
			show_module_lab_menu()
		GameState.ARCHIVES:
			show_coming_soon("ARCHIVES")
		GameState.SYSTEM:
			show_coming_soon("SYSTEM SETTINGS")

func show_module_lab_menu():
	_terminal.clear()
	_terminal.print_line("=== MODULE LAB ===", Terminal.MessageType.SYSTEM)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("[1] AI ASSEMBLY - Combine modules into AI", Terminal.MessageType.NORMAL)
	_terminal.print_line("[2] MODULE DATABASE - Browse all modules", Terminal.MessageType.NORMAL)
	_terminal.print_line("[3] BACK TO MAIN MENU", Terminal.MessageType.NORMAL)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("Select option (1-3)", Terminal.MessageType.SUCCESS)
	
	_current_state = GameState.MODULE_LAB_MENU

func show_coming_soon(feature: String):
	_terminal.clear()
	_terminal.print_line("=== %s ===" % feature, Terminal.MessageType.SYSTEM)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("This feature is under development.", Terminal.MessageType.WARNING)
	_terminal.print_line("Check back in a future update.", Terminal.MessageType.NORMAL)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("Tap to return to MAIN TERMINAL", Terminal.MessageType.SYSTEM)

func _on_viewport_size_changed():
	update_input_area_size()

func update_input_area_size():
	if ResolutionManager.get_instance() != null:
		# Input area should be about 12% of screen height
		var input_area_height = ResolutionManager.get_percentage_of_screen_height(12.0)
		# Minimum 80 pixels for touch accessibility
		input_area_height = max(80.0, input_area_height)
		
		# Update the input area size - InputArea is anchored to bottom
		# So we adjust the top offset to control height
		offset_top = -input_area_height
		
		print("TerminalInput: Input area height set to %.0fpx" % input_area_height)

func _on_tap_detected():
	match _current_state:
		GameState.STARTUP:
			_current_state = GameState.MAIN_MENU
			show_main_menu()
		
		GameState.MAIN_MENU:
			# Don't auto-advance to battle on tap in main menu
			# Wait for specific menu selection
			pass
		
		GameState.PRE_BATTLE:
			_current_state = GameState.BATTLE
			start_battle()
		
		GameState.BATTLE:
			_current_state = GameState.POST_BATTLE
			show_battle_results()
		
		GameState.POST_BATTLE:
			_current_state = GameState.MAIN_MENU
			show_main_menu()
		
		GameState.MODULE_LAB, GameState.MODULE_LAB_MENU, GameState.ARCHIVES, GameState.SYSTEM:
			# Return to main menu from these states
			_current_state = GameState.MAIN_MENU
			show_main_menu()

func transition_to_main_menu():
	_current_state = GameState.MAIN_MENU
	show_main_menu()

func show_main_menu():
	_terminal.clear()
	_terminal.print_line("=== MAIN TERMINAL ===", Terminal.MessageType.SYSTEM)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	
	for option in _main_menu_options:
		_terminal.print_line("[%s] %s" % [option.key, option.label], Terminal.MessageType.NORMAL)
	
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("Select option (1-4) or tap to return", Terminal.MessageType.SUCCESS)

func show_pre_battle():
	_terminal.clear()
	_terminal.print_line("=== PRE-BATTLE CONFIGURATION ===", Terminal.MessageType.SYSTEM)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("Current AI Configuration:", Terminal.MessageType.NORMAL)
	_terminal.print_line("  Core: ALPHA_CORE v2.1", Terminal.MessageType.SUCCESS)
	_terminal.print_line("  Behavior: AGGRESSIVE_STANCE", Terminal.MessageType.SUCCESS)
	_terminal.print_line("  Augment: BUFFER_OVERFLOW", Terminal.MessageType.SUCCESS)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("Enemy Detected:", Terminal.MessageType.WARNING)
	_terminal.print_line("  Type: WILD_AI_FRAGMENT", Terminal.MessageType.ERROR)
	_terminal.print_line("  Threat Level: MODERATE", Terminal.MessageType.WARNING)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("Tap to ENGAGE", Terminal.MessageType.SYSTEM)

func start_battle():
	_terminal.show_battle_intro("ALPHA_BUILD", "CORRUPTED_DAEMON")
	
	call_deferred("simulate_battle")

func simulate_battle():
	var timer = get_tree().create_timer(2.0)
	timer.timeout.connect(func():
		_terminal.print_line("ROUND 1:", Terminal.MessageType.SYSTEM)
		_terminal.print_line("  > Your AI executes BUFFER_OVERFLOW", Terminal.MessageType.SUCCESS)
		_terminal.print_line("  > Enemy takes 15 damage", Terminal.MessageType.NORMAL)
		_terminal.print_line("  > Enemy executes MALWARE_INJECTION", Terminal.MessageType.ERROR)
		_terminal.print_line("  > You take 12 damage", Terminal.MessageType.WARNING)
		_terminal.print_line("", Terminal.MessageType.NORMAL)
		
		var timer2 = get_tree().create_timer(2.0)
		timer2.timeout.connect(func():
			_terminal.print_line("ROUND 2:", Terminal.MessageType.SYSTEM)
			_terminal.print_line("  > Your AI executes AGGRESSIVE_STRIKE", Terminal.MessageType.SUCCESS)
			_terminal.print_line("  > CRITICAL HIT! Enemy takes 25 damage", Terminal.MessageType.SUCCESS)
			_terminal.print_line("  > Enemy system corrupted", Terminal.MessageType.ERROR)
			_terminal.print_line("", Terminal.MessageType.NORMAL)
			_terminal.print_line("=== VICTORY ===", Terminal.MessageType.SUCCESS)
			_terminal.print_line("", Terminal.MessageType.NORMAL)
			_terminal.print_line("Tap to continue...", Terminal.MessageType.SYSTEM)
		)
	)

func show_battle_results():
	_terminal.clear()
	_terminal.print_line("=== BATTLE COMPLETE ===", Terminal.MessageType.SUCCESS)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("SALVAGE AVAILABLE:", Terminal.MessageType.SYSTEM)
	_terminal.print_line("  > MALWARE_INJECTION (Augment)", Terminal.MessageType.WARNING)
	_terminal.print_line("  > 15 BITS earned", Terminal.MessageType.SUCCESS)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("AI PERFORMANCE:", Terminal.MessageType.SYSTEM)
	_terminal.print_line("  Damage Dealt: 40", Terminal.MessageType.NORMAL)
	_terminal.print_line("  Damage Taken: 12", Terminal.MessageType.NORMAL)
	_terminal.print_line("  Efficiency: 77%", Terminal.MessageType.SUCCESS)
	_terminal.print_line("", Terminal.MessageType.NORMAL)
	_terminal.print_line("Tap to return to MAIN TERMINAL", Terminal.MessageType.SYSTEM)

func transition_to_ai_assembly():
	_terminal.clear()
	_terminal.print_line("=== ACCESSING AI ASSEMBLY ===", Terminal.MessageType.SYSTEM)
	_terminal.print_line("Loading assembly interface...", Terminal.MessageType.NORMAL)
	
	var timer = get_tree().create_timer(1.0)
	timer.timeout.connect(func():
		get_tree().change_scene_to_file("res://Scenes/AIAssembly.tscn")
	)

func transition_to_module_viewer():
	_terminal.clear()
	_terminal.print_line("=== ACCESSING MODULE DATABASE ===", Terminal.MessageType.SYSTEM)
	_terminal.print_line("Loading module viewer...", Terminal.MessageType.NORMAL)
	
	var timer = get_tree().create_timer(1.0)
	timer.timeout.connect(func():
		get_tree().change_scene_to_file("res://Scenes/ModuleViewer.tscn")
	)
