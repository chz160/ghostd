extends Control

var _module_display: RichTextLabel
var _previous_button: Button
var _next_button: Button
var _module_type_label: Label
var _module_count_label: Label
var _core_button: Button
var _behavior_button: Button
var _augment_button: Button
var _back_button: Button

var _all_modules: Array[ModuleBase] = []
var _current_index = 0
var _current_type = ModuleBase.ModuleType.CORE

func _ready():
	# Get node references
	_module_display = get_node("Container/ScrollContainer/ModuleDisplay")
	_previous_button = get_node("Container/Navigation/PreviousButton")
	_next_button = get_node("Container/Navigation/NextButton")
	_module_type_label = get_node("Container/ModuleInfo/ModuleTypeLabel")
	_module_count_label = get_node("Container/ModuleInfo/ModuleCountLabel")
	_core_button = get_node("Container/TypeButtons/CoreButton")
	_behavior_button = get_node("Container/TypeButtons/BehaviorButton")
	_augment_button = get_node("Container/TypeButtons/AugmentButton")
	_back_button = get_node("Container/BackButton")
	
	# Connect buttons
	if _previous_button != null:
		_previous_button.pressed.connect(_on_previous_pressed)
	if _next_button != null:
		_next_button.pressed.connect(_on_next_pressed)
	if _core_button != null:
		_core_button.pressed.connect(func(): switch_to_type(ModuleBase.ModuleType.CORE))
	if _behavior_button != null:
		_behavior_button.pressed.connect(func(): switch_to_type(ModuleBase.ModuleType.BEHAVIOR))
	if _augment_button != null:
		_augment_button.pressed.connect(func(): switch_to_type(ModuleBase.ModuleType.AUGMENT))
	if _back_button != null:
		_back_button.pressed.connect(return_to_main_terminal)
	
	# Load modules after a short delay to ensure ModuleDatabase is ready
	get_tree().create_timer(0.1).timeout.connect(load_modules)

func load_modules():
	if ModuleDatabase.get_instance() == null:
		push_error("ModuleViewer: ModuleDatabase not ready!")
		return
	
	# Load all modules
	refresh_module_list()
	display_current_module()

func refresh_module_list():
	_all_modules.clear()
	
	match _current_type:
		ModuleBase.ModuleType.CORE:
			_all_modules.append_array(ModuleDatabase.get_instance().get_core_modules())
		ModuleBase.ModuleType.BEHAVIOR:
			_all_modules.append_array(ModuleDatabase.get_instance().get_behavior_modules())
		ModuleBase.ModuleType.AUGMENT:
			_all_modules.append_array(ModuleDatabase.get_instance().get_augment_modules())
	
	_current_index = 0
	update_labels()

func update_labels():
	if _module_type_label != null:
		_module_type_label.text = "TYPE: %s" % ModuleBase.ModuleType.keys()[_current_type]
	
	if _module_count_label != null and _all_modules.size() > 0:
		_module_count_label.text = "[%d/%d]" % [_current_index + 1, _all_modules.size()]
	elif _module_count_label != null:
		_module_count_label.text = "[0/0]"

func display_current_module():
	if _module_display == null:
		return
	
	_module_display.clear()
	
	if _all_modules.size() == 0:
		_module_display.append_text("[color=#ff4444]NO MODULES FOUND[/color]\n")
		_module_display.append_text("Module type: %s\n" % ModuleBase.ModuleType.keys()[_current_type])
		return
	
	if _current_index >= 0 and _current_index < _all_modules.size():
		var module = _all_modules[_current_index]
		_module_display.append_text(module.get_terminal_display())
		
		# Add type-specific information
		if module is CoreModule:
			display_core_specifics(module)
		elif module is BehaviorModule:
			display_behavior_specifics(module)
		elif module is AugmentModule:
			display_augment_specifics(module)
	
	update_labels()

func display_core_specifics(core: CoreModule):
	_module_display.append_text("\n[color=#00ff00]BOOT SEQUENCE:[/color]\n")
	_module_display.append_text(core.get_boot_sequence())

func display_behavior_specifics(behavior: BehaviorModule):
	_module_display.append_text("\n[color=#00ff00]COMBAT PREVIEW:[/color]\n")
	_module_display.append_text("Attack Action: %s\n" % behavior.get_combat_message(BehaviorModule.BattleAction.ATTACK))
	_module_display.append_text("Defend Action: %s\n" % behavior.get_combat_message(BehaviorModule.BattleAction.DEFEND))
	_module_display.append_text("Special Action: %s\n" % behavior.get_combat_message(BehaviorModule.BattleAction.SPECIAL))

func display_augment_specifics(augment: AugmentModule):
	_module_display.append_text("\n[color=#00ff00]ACTIVATION PREVIEW:[/color]\n")
	_module_display.append_text(augment.get_activation_log("TEST_AI", "ENEMY_AI"))

func _on_previous_pressed():
	if _all_modules.size() == 0:
		return
	
	_current_index -= 1
	if _current_index < 0:
		_current_index = _all_modules.size() - 1
	
	display_current_module()

func _on_next_pressed():
	if _all_modules.size() == 0:
		return
	
	_current_index += 1
	if _current_index >= _all_modules.size():
		_current_index = 0
	
	display_current_module()

func switch_to_type(type: ModuleBase.ModuleType):
	_current_type = type
	refresh_module_list()
	display_current_module()

# Input handling for keyboard navigation
func _input(event):
	if event is InputEventKey and event.pressed:
		match event.keycode:
			KEY_LEFT, KEY_A:
				_on_previous_pressed()
			KEY_RIGHT, KEY_D:
				_on_next_pressed()
			KEY_1:
				switch_to_type(ModuleBase.ModuleType.CORE)
			KEY_2:
				switch_to_type(ModuleBase.ModuleType.BEHAVIOR)
			KEY_3:
				switch_to_type(ModuleBase.ModuleType.AUGMENT)
			KEY_ESCAPE:
				return_to_main_terminal()

func return_to_main_terminal():
	get_tree().change_scene_to_file("res://Scenes/MainTerminal.tscn")
