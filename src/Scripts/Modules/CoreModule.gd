extends ModuleBase
class_name CoreModule

## Core modules define the base stats and foundation of an AI

# Core-specific properties
@export var base_health: int = 100
@export var base_processing_power: int = 10
@export var stability_rating: float = 1.0
@export var core_architecture: String = "Standard"

# Core modules always have ModuleType.Core
func _init():
	type = ModuleType.CORE

## Calculate total health including modifiers and corruption
func get_total_health() -> int:
	var corruption_multiplier = 1.0 - (corruption_level * 0.2) # Corruption reduces health
	return max(1, roundi(base_health * corruption_multiplier * stability_rating))

## Calculate total processing power
func get_total_processing_power() -> int:
	var corruption_bonus = 1.0 + (corruption_level * 0.3) # Corruption increases power
	return roundi(base_processing_power * corruption_bonus)

func get_terminal_display() -> String:
	var display = super.get_terminal_display()
	
	# Add core-specific stats
	display += "\nCORE SPECIFICATIONS:\n"
	display += "  Health Pool: %d HP\n" % get_total_health()
	display += "  Processing Power: %d PWR\n" % get_total_processing_power()
	display += "  Stability: %d%%\n" % int(stability_rating * 100)
	display += "  Architecture: %s\n" % core_architecture
	
	return display

## Core modules can have special initialization sequences
func get_boot_sequence() -> String:
	return "Initializing %s...\n" % module_name + \
		   "Loading %s architecture...\n" % core_architecture + \
		   "Stability check: %s\n" % ("PASSED" if stability_rating >= 0.8 else "WARNING") + \
		   "Core online."
