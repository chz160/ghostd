extends Control

# UI References
var _assembly_display: RichTextLabel
var _stats_display: RichTextLabel
var _core_slot_button: Button
var _behavior_slot_button: Button
var _augment_slot_button: Button
var _assemble_button: Button
var _clear_button: Button
var _status_label: Label
var _back_button: Button

# Module Selection
var _module_selection_panel: Panel
var _module_list: ItemList
var _module_preview: RichTextLabel
var _select_button: Button
var _cancel_button: Button

# Current Assembly
var _selected_core: CoreModule
var _selected_behavior: BehaviorModule
var _selected_augment: AugmentModule
var _selecting_type: ModuleBase.ModuleType

# Assembled AI stats
var _assembled_stats = {}
	
func _ready():
	# Get UI references
	_assembly_display = get_node("Container/DisplayContainer/AssemblyPanel/AssemblyDisplay")
	_stats_display = get_node("Container/DisplayContainer/StatsPanel/StatsDisplay")
	_core_slot_button = get_node("Container/ModuleSlots/CoreSlot")
	_behavior_slot_button = get_node("Container/ModuleSlots/BehaviorSlot")
	_augment_slot_button = get_node("Container/ModuleSlots/AugmentSlot")
	_assemble_button = get_node("Container/Actions/AssembleButton")
	_clear_button = get_node("Container/Actions/ClearButton")
	_status_label = get_node("Container/StatusLabel")
	_back_button = get_node("Container/BackButton")
	
	# Module selection panel
	_module_selection_panel = get_node("ModuleSelectionPanel")
	_module_list = get_node("ModuleSelectionPanel/VBox/ModuleList")
	_module_preview = get_node("ModuleSelectionPanel/VBox/ModulePreview")
	_select_button = get_node("ModuleSelectionPanel/VBox/Actions/SelectButton")
	_cancel_button = get_node("ModuleSelectionPanel/VBox/Actions/CancelButton")
	
	# Connect buttons
	_core_slot_button.pressed.connect(func(): open_module_selection(ModuleBase.ModuleType.CORE))
	_behavior_slot_button.pressed.connect(func(): open_module_selection(ModuleBase.ModuleType.BEHAVIOR))
	_augment_slot_button.pressed.connect(func(): open_module_selection(ModuleBase.ModuleType.AUGMENT))
	_assemble_button.pressed.connect(on_assemble_pressed)
	_clear_button.pressed.connect(on_clear_pressed)
	_select_button.pressed.connect(on_select_module_pressed)
	_cancel_button.pressed.connect(on_cancel_selection_pressed)
	_module_list.item_selected.connect(on_module_list_item_selected)
	_back_button.pressed.connect(return_to_main_terminal)
	
	# Initialize
	_module_selection_panel.visible = false
	update_display()
	
func open_module_selection(type: ModuleBase.ModuleType):
	_selecting_type = type
	_module_selection_panel.visible = true
	_module_list.clear()
	_module_preview.clear()
	
	# Populate module list
	var modules = []
	match type:
		ModuleBase.ModuleType.CORE:
			modules = ModuleDatabase.get_instance().get_core_modules()
		ModuleBase.ModuleType.BEHAVIOR:
			modules = ModuleDatabase.get_instance().get_behavior_modules()
		ModuleBase.ModuleType.AUGMENT:
			modules = ModuleDatabase.get_instance().get_augment_modules()
	
	for module in modules:
		var item_text = "%s [%s]" % [module.module_name, ModuleBase.ModuleRarity.keys()[module.rarity]]
		_module_list.add_item(item_text)
		
		# Store module reference in metadata
		_module_list.set_item_metadata(_module_list.get_item_count() - 1, module)
		
		# Color based on rarity
		var color = module.get_rarity_color()
		_module_list.set_item_custom_fg_color(_module_list.get_item_count() - 1, color)
	
	_status_label.text = "Select a %s module..." % ModuleBase.ModuleType.keys()[type]
	
func on_module_list_item_selected(index: int):
	var module = _module_list.get_item_metadata(index)
	if module != null:
		_module_preview.clear()
		_module_preview.append_text(module.get_terminal_display())
	
func on_select_module_pressed():
	var selected_items = _module_list.get_selected_items()
	if selected_items.size() == 0:
		return
	
	var module = _module_list.get_item_metadata(selected_items[0])
	if module == null:
		return
	
	# Assign to appropriate slot
	match _selecting_type:
		ModuleBase.ModuleType.CORE:
			_selected_core = module
		ModuleBase.ModuleType.BEHAVIOR:
			_selected_behavior = module
		ModuleBase.ModuleType.AUGMENT:
			_selected_augment = module
	
	_module_selection_panel.visible = false
	update_display()
	update_slot_buttons()
	
func on_cancel_selection_pressed():
	_module_selection_panel.visible = false
	_status_label.text = "Module selection cancelled."
	
func update_slot_buttons():
	# Update button text to show selected modules
	_core_slot_button.text = "CORE: %s" % _selected_core.module_name if _selected_core != null else "CORE: [EMPTY]"
	_behavior_slot_button.text = "BEHAVIOR: %s" % _selected_behavior.module_name if _selected_behavior != null else "BEHAVIOR: [EMPTY]"
	_augment_slot_button.text = "AUGMENT: %s" % _selected_augment.module_name if _selected_augment != null else "AUGMENT: [EMPTY]"
	
	# Enable assemble button if all slots filled
	_assemble_button.disabled = _selected_core == null or _selected_behavior == null or _selected_augment == null
	
func update_display():
	update_slot_buttons()
	update_assembly_display()
	update_stats_display()
	
func update_assembly_display():
	_assembly_display.clear()
	
	_assembly_display.append_text("[color=#00ff00]═══ AI ASSEMBLY ═══[/color]\n\n")
	
	# Core
	_assembly_display.append_text("[color=#00ffff]CORE MODULE:[/color]\n")
	if _selected_core != null:
		_assembly_display.append_text("  %s\n" % _selected_core.module_name)
		_assembly_display.append_text("  Architecture: %s\n" % _selected_core.core_architecture)
		_assembly_display.append_text("  Base HP: %d\n" % _selected_core.get_total_health())
		_assembly_display.append_text("  Base PWR: %d\n" % _selected_core.get_total_processing_power())
	else:
		_assembly_display.append_text("  [color=#666666][EMPTY SLOT][/color]\n")
	
	_assembly_display.append_text("\n")
	
	# Behavior
	_assembly_display.append_text("[color=#00ffff]BEHAVIOR MODULE:[/color]\n")
	if _selected_behavior != null:
		_assembly_display.append_text("  %s\n" % _selected_behavior.module_name)
		_assembly_display.append_text("  Pattern: %s\n" % _selected_behavior.primary_pattern)
		_assembly_display.append_text("  Aggression: %d%%\n" % int(_selected_behavior.aggression_level * 100))
	else:
		_assembly_display.append_text("  [color=#666666][EMPTY SLOT][/color]\n")
	
	_assembly_display.append_text("\n")
	
	# Augment
	_assembly_display.append_text("[color=#00ffff]AUGMENT MODULE:[/color]\n")
	if _selected_augment != null:
		_assembly_display.append_text("  %s\n" % _selected_augment.module_name)
		_assembly_display.append_text("  Ability: %s\n" % _selected_augment.ability_name)
		_assembly_display.append_text("  Cost: %d PWR\n" % _selected_augment.power_cost)
	else:
		_assembly_display.append_text("  [color=#666666][EMPTY SLOT][/color]\n")
	
	# ASCII art preview
	if _selected_core != null and _selected_behavior != null and _selected_augment != null:
		_assembly_display.append_text("\n[color=#00ff00]═══ AI PREVIEW ═══[/color]\n")
		_assembly_display.append_text(generate_ai_ascii_art())
	
func update_stats_display():
	_stats_display.clear()
	
	_stats_display.append_text("[color=#00ff00]═══ COMBINED STATS ═══[/color]\n\n")
	
	if _selected_core == null or _selected_behavior == null or _selected_augment == null:
		_stats_display.append_text("[color=#666666]Select all modules to see combined stats[/color]\n")
		return
	
	# Calculate combined stats
	calculate_combined_stats()
	
	# Display stats
	_stats_display.append_text("[color=#ff6666]HEALTH:[/color] %d\n" % _assembled_stats["HP"])
	_stats_display.append_text("[color=#6666ff]POWER:[/color] %d\n" % _assembled_stats["PWR"])
	_stats_display.append_text("[color=#ff9966]ATTACK:[/color] %d\n" % _assembled_stats["ATK"])
	_stats_display.append_text("[color=#66ff66]DEFENSE:[/color] %d\n" % _assembled_stats["DEF"])
	_stats_display.append_text("[color=#ffff66]SPEED:[/color] %d\n" % _assembled_stats["SPD"])
	
	# Stability and corruption
	var total_corruption = _selected_core.corruption_level + _selected_behavior.corruption_level + _selected_augment.corruption_level
	if total_corruption > 0:
		_stats_display.append_text("\n[color=#ff4444]CORRUPTION:[/color] %d%%\n" % int(total_corruption * 100))
	
	_stats_display.append_text("\n[color=#00ffff]STABILITY:[/color] %d%%\n" % int(_selected_core.stability_rating * 100))
	
	# Special properties
	_stats_display.append_text("\n[color=#00ff00]═══ PROPERTIES ═══[/color]\n")
	
	var all_tags = []
	all_tags.append_array(_selected_core.tags)
	all_tags.append_array(_selected_behavior.tags)
	all_tags.append_array(_selected_augment.tags)
	
	for tag in all_tags:
		_stats_display.append_text("• %s\n" % tag)
	
func calculate_combined_stats():
	_assembled_stats.clear()
	
	# Base stats from core
	_assembled_stats["HP"] = _selected_core.get_total_health()
	_assembled_stats["PWR"] = _selected_core.get_total_processing_power()
	
	# Combine modifiers from all modules
	_assembled_stats["ATK"] = 10 + _selected_core.attack_modifier + _selected_behavior.attack_modifier + _selected_augment.attack_modifier
	_assembled_stats["DEF"] = 10 + _selected_core.defense_modifier + _selected_behavior.defense_modifier + _selected_augment.defense_modifier
	_assembled_stats["SPD"] = 10 + _selected_core.speed_modifier + _selected_behavior.speed_modifier + _selected_augment.speed_modifier
	
	# Apply behavior modifiers
	if _selected_behavior.aggression_level > 0.7:
		_assembled_stats["ATK"] = roundi(_assembled_stats["ATK"] * 1.2)
	elif _selected_behavior.aggression_level < 0.3:
		_assembled_stats["DEF"] = roundi(_assembled_stats["DEF"] * 1.2)
	
	# Ensure minimum values
	for key in _assembled_stats.keys():
		if _assembled_stats[key] < 1:
			_assembled_stats[key] = 1
	
func generate_ai_ascii_art() -> String:
	# Combine ASCII art from modules creatively
	return """
	╔═══════╗
	║  ◊-◊  ║
	║ ┌───┐ ║
	║ │ AI│ ║
	║ └───┘ ║
	╚═══════╝
"""
	
func on_assemble_pressed():
	if _selected_core == null or _selected_behavior == null or _selected_augment == null:
		_status_label.text = "ERROR: All module slots must be filled!"
		return
	
	_status_label.text = "AI ASSEMBLED SUCCESSFULLY! Ready for deployment."
	
	# TODO: Save assembled AI configuration
	# TODO: Transition to battle or main menu
	
func on_clear_pressed():
	_selected_core = null
	_selected_behavior = null
	_selected_augment = null
	update_display()
	_status_label.text = "Assembly cleared. Select new modules."
	
# Input handling for escape key
func _input(event):
	if event is InputEventKey and event.pressed:
		if event.keycode == KEY_ESCAPE:
			return_to_main_terminal()

func return_to_main_terminal():
	get_tree().change_scene_to_file("res://Scenes/MainTerminal.tscn")
