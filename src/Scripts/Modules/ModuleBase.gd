extends Resource
class_name ModuleBase

## Base class for all AI modules in the game

# Module type enumeration
enum ModuleType {
	CORE,
	BEHAVIOR,
	AUGMENT
}

# Module rarity levels
enum ModuleRarity {
	COMMON,
	UNCOMMON,
	RARE,
	EPIC,
	LEGENDARY,
	CORRUPTED
}

@export var module_id: String = ""
@export var module_name: String = "Unknown Module"
@export var description: String = ""
@export var type: ModuleType
@export var rarity: ModuleRarity = ModuleRarity.COMMON

# Base stats that all modules can modify
@export var attack_modifier: int = 0
@export var defense_modifier: int = 0
@export var speed_modifier: int = 0
@export var power_modifier: int = 0

# ASCII art representation (for terminal display)
@export_multiline var ascii_art: String = ""

# Flavor text for terminal display
@export_multiline var flavor_text: String = ""

# Corruption level (affects stats and behavior)
@export var corruption_level: float = 0.0

# Tags for special properties
@export var tags: Array[String] = []

## Get the display color for this module based on rarity
func get_rarity_color() -> Color:
	match rarity:
		ModuleRarity.COMMON:
			return Color(0.7, 0.7, 0.7) # Gray
		ModuleRarity.UNCOMMON:
			return Color(0.2, 1.0, 0.2) # Green
		ModuleRarity.RARE:
			return Color(0.2, 0.6, 1.0) # Blue
		ModuleRarity.EPIC:
			return Color(0.8, 0.2, 1.0) # Purple
		ModuleRarity.LEGENDARY:
			return Color(1.0, 0.8, 0.2) # Gold
		ModuleRarity.CORRUPTED:
			return Color(1.0, 0.2, 0.2) # Red
		_:
			return Color(1.0, 1.0, 1.0) # White

## Get formatted terminal display text for this module
func get_terminal_display() -> String:
	var color = get_rarity_color()
	var hex_color = "#%02X%02X%02X" % [int(color.r * 255), int(color.g * 255), int(color.b * 255)]
	
	var display = "[color=%s]══════════════════════════════════════[/color]\n" % hex_color
	display += "[color=%s]MODULE: %s[/color]\n" % [hex_color, module_name.to_upper()]
	display += "[color=%s]TYPE: %s | RARITY: %s[/color]\n" % [hex_color, ModuleType.keys()[type], ModuleRarity.keys()[rarity]]
	display += "[color=%s]══════════════════════════════════════[/color]\n" % hex_color
	
	if not ascii_art.is_empty():
		display += "%s\n" % ascii_art
	
	display += "\n%s\n\n" % description
	
	# Show stat modifiers
	display += "STAT MODIFIERS:\n"
	if attack_modifier != 0:
		display += "  ATK: %s%d\n" % ["+" if attack_modifier > 0 else "", attack_modifier]
	if defense_modifier != 0:
		display += "  DEF: %s%d\n" % ["+" if defense_modifier > 0 else "", defense_modifier]
	if speed_modifier != 0:
		display += "  SPD: %s%d\n" % ["+" if speed_modifier > 0 else "", speed_modifier]
	if power_modifier != 0:
		display += "  PWR: %s%d\n" % ["+" if power_modifier > 0 else "", power_modifier]
	
	if corruption_level > 0:
		display += "\n[color=#ff4444]CORRUPTION: %d%%[/color]\n" % int(corruption_level * 100)
	
	if not flavor_text.is_empty():
		display += "\n[i]%s[/i]\n" % flavor_text
	
	return display

## Apply corruption effects to the module
func apply_corruption(amount: float) -> void:
	corruption_level = clamp(corruption_level + amount, 0.0, 1.0)
	
	# Corruption can boost stats but at a cost
	if corruption_level > 0.5:
		attack_modifier = roundi(attack_modifier * (1 + corruption_level))
		# Add instability or other negative effects

## Check if this module has a specific tag
func has_tag(tag: String) -> bool:
	for t in tags:
		if t.to_lower() == tag.to_lower():
			return true
	return false
