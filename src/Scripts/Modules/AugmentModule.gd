extends ModuleBase
class_name AugmentModule

## Augment modules provide special abilities and unique effects

enum TargetType {
	SELF,
	ENEMY,
	ALL,
	RANDOM
}

# Augment-specific properties
@export var ability_name: String = "Unknown Ability"
@export var ability_description: String = ""
@export var cooldown_turns: float = 3
@export var power_cost: float = 20
@export var targeting_mode: TargetType = TargetType.ENEMY

# Effect properties
@export var effect_power: float = 1.0
@export var effect_duration: float = 0 # 0 = instant
@export var effect_tags: Array[String] = []

# Activation conditions
@export var activation_condition: String = "Manual"
@export var auto_trigger_threshold: float = 0.0

# Visual/Audio feedback
@export var activation_message: String = "AUGMENT ACTIVATED"
@export var effect_animation: String = "default"

# Augment modules always have ModuleType.Augment
func _init():
	type = ModuleType.AUGMENT

## Check if the augment can be activated
func can_activate(current_power: int, current_cooldown: int, health_percent: float = 1.0) -> bool:
	# Check power cost
	if current_power < power_cost:
		return false
	
	# Check cooldown
	if current_cooldown > 0:
		return false
	
	# Check auto-trigger conditions
	if activation_condition == "LowHealth" and health_percent > auto_trigger_threshold:
		return false
	
	return true

## Calculate the actual effect value considering corruption and other modifiers
func get_effect_value() -> float:
	# Corruption can enhance augment effects but make them unstable
	var corruption_bonus = 1.0 + (corruption_level * 0.5)
	var instability = corruption_level > 0.5 if randf() * 0.2 - 0.1 else 0.0
	
	return effect_power * corruption_bonus * (1.0 + instability)

func get_terminal_display() -> String:
	var display = super.get_terminal_display()
	
	# Add augment-specific info
	display += "\nAUGMENT SPECIFICATIONS:\n"
	display += "  Ability: %s\n" % ability_name
	display += "  %s\n\n" % ability_description
	
	display += "  PARAMETERS:\n"
	display += "    Power Cost: %d PWR\n" % int(power_cost)
	display += "    Cooldown: %d turns\n" % int(cooldown_turns)
	display += "    Target: %s\n" % TargetType.keys()[targeting_mode]
	display += "    Effect Power: %.1f\n" % get_effect_value()
	
	if effect_duration > 0:
		display += "    Duration: %d turns\n" % int(effect_duration)
	
	if activation_condition != "Manual":
		display += "\n  AUTO-TRIGGER: %s" % activation_condition
		if auto_trigger_threshold > 0:
			display += " (< %d%%)" % int(auto_trigger_threshold * 100)
		display += "\n"
	
	if effect_tags.size() > 0:
		display += "\n  EFFECT TAGS: "
		display += ", ".join(effect_tags)
		display += "\n"
	
	return display

## Get the activation message for the battle log
func get_activation_log(user_name: String, target_name: String) -> String:
	var log = "[color=#00ffff]%s ACTIVATES %s![/color]\n" % [user_name, ability_name.to_upper()]
	log += "> %s\n" % activation_message
	
	if targeting_mode == TargetType.ENEMY:
		log += "> TARGET: %s\n" % target_name
	elif targeting_mode == TargetType.SELF:
		log += "> SELF-TARGETED PROTOCOL\n"
	
	# Add effect description
	for tag in effect_tags:
		log += get_effect_description(tag)
	
	return log

func get_effect_description(effect_tag: String) -> String:
	match effect_tag.to_lower():
		"damage":
			return "> DAMAGE OUTPUT: %.0f\n" % get_effect_value()
		"heal":
			return "> REPAIR SYSTEMS: +%.0f HP\n" % get_effect_value()
		"shield":
			return "> DEFENSIVE MATRIX: +%.0f DEF\n" % get_effect_value()
		"drain":
			return "> ENERGY DRAIN: %.0f\n" % get_effect_value()
		"stun":
			return "> SYSTEM DISRUPTION: %d CYCLES\n" % int(effect_duration)
		"boost":
			return "> PERFORMANCE BOOST: +%.0f%%\n" % get_effect_value()
		_:
			return ""

## Apply the augment's effect
func apply_effect(user: Dictionary, target: Dictionary) -> Dictionary:
	var effect = {
		"source_augment": self,
		"value": get_effect_value(),
		"duration": effect_duration,
		"tags": effect_tags,
		"damage_dealt": 0,
		"healing_done": 0,
		"shield_amount": 0,
		"stat_modifiers": {}
	}
	
	# Apply immediate effects based on tags
	for tag in effect_tags:
		apply_effect_by_tag(tag, effect, user, target)
	
	return effect

func apply_effect_by_tag(tag: String, effect: Dictionary, user: Dictionary, target: Dictionary) -> void:
	match tag.to_lower():
		"damage":
			if targeting_mode == TargetType.ENEMY:
				effect.damage_dealt = roundi(effect.value)
		"heal":
			if targeting_mode == TargetType.SELF:
				effect.healing_done = roundi(effect.value)
		"shield":
			effect.shield_amount = roundi(effect.value)
		"boost":
			effect.stat_modifiers["ATK"] = effect.value * 0.01

## Helper class for augment effects
class AugmentEffect:
	var source_augment: AugmentModule
	var value: float
	var duration: float
	var tags: Array[String]
	
	# Effect results
	var damage_dealt: int = 0
	var healing_done: int = 0
	var shield_amount: int = 0
	var stat_modifiers: Dictionary = {}
	
	func is_expired(current_turn: int, applied_turn: int) -> bool:
		return duration > 0 and (current_turn - applied_turn) >= duration
