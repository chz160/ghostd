extends ModuleBase
class_name BehaviorModule

## Behavior modules define combat patterns and AI decision-making

enum BattleAction {
	ATTACK,
	DEFEND,
	SPECIAL,
	WAIT
}

# Behavior-specific properties
@export var aggression_level: float = 0.5 # 0.0 = defensive, 1.0 = aggressive
@export var adaptability: float = 0.5 # How well it responds to enemy patterns
@export var primary_pattern: String = "Balanced"
@export var combat_routines: Array[String] = []

# Behavior weights for different actions
@export var attack_weight: float = 0.33
@export var defend_weight: float = 0.33
@export var special_weight: float = 0.34

# Behavior modules always have ModuleType.Behavior
func _init():
	type = ModuleType.BEHAVIOR

## Get the next action based on behavior weights and current battle state
func get_next_action(health_percent: float, enemy_health_percent: float, turn_number: int) -> BattleAction:
	# Adjust weights based on battle state
	var adjusted_attack_weight = attack_weight
	var adjusted_defend_weight = defend_weight
	var adjusted_special_weight = special_weight
	
	# More aggressive when enemy is low health
	if enemy_health_percent < 0.3:
		adjusted_attack_weight *= (1 + aggression_level)
	
	# More defensive when own health is low
	if health_percent < 0.3:
		adjusted_defend_weight *= (2 - aggression_level)
	
	# Normalize weights
	var total_weight = adjusted_attack_weight + adjusted_defend_weight + adjusted_special_weight
	adjusted_attack_weight /= total_weight
	adjusted_defend_weight /= total_weight
	adjusted_special_weight /= total_weight
	
	# Random selection based on weights
	var random = randf()
	if random < adjusted_attack_weight:
		return BattleAction.ATTACK
	elif random < adjusted_attack_weight + adjusted_defend_weight:
		return BattleAction.DEFEND
	else:
		return BattleAction.SPECIAL

func get_terminal_display() -> String:
	var display = super.get_terminal_display()
	
	# Add behavior-specific info
	display += "\nBEHAVIOR PROFILE:\n"
	display += "  Pattern: %s\n" % primary_pattern
	display += "  Aggression: %s\n" % get_aggression_bar()
	display += "  Adaptability: %d%%\n" % int(adaptability * 100)
	
	if combat_routines.size() > 0:
		display += "\n  Combat Routines:\n"
		for routine in combat_routines:
			display += "    - %s\n" % routine
	
	return display

func get_aggression_bar() -> String:
	var bar_length = 10
	var filled_length = roundi(aggression_level * bar_length)
	var bar = "["
	
	for i in bar_length:
		if i < filled_length:
			bar += "█"
		else:
			bar += "░"
	
	bar += "] %d%%" % int(aggression_level * 100)
	return bar

## Get a combat taunt/message based on behavior
func get_combat_message(action: BattleAction) -> String:
	if aggression_level > 0.7:
		match action:
			BattleAction.ATTACK:
				return "EXECUTING OFFENSIVE PROTOCOL"
			BattleAction.DEFEND:
				return "TEMPORARY DEFENSIVE STANCE"
			BattleAction.SPECIAL:
				return "UNLEASHING MAXIMUM FORCE"
			_:
				return "ANALYZING TARGET"
	elif aggression_level < 0.3:
		match action:
			BattleAction.ATTACK:
				return "CALCULATED STRIKE"
			BattleAction.DEFEND:
				return "FORTIFYING DEFENSES"
			BattleAction.SPECIAL:
				return "TACTICAL MANEUVER"
			_:
				return "EVALUATING OPTIONS"
	else:
		match action:
			BattleAction.ATTACK:
				return "ENGAGING TARGET"
			BattleAction.DEFEND:
				return "DEFENSIVE MEASURES ACTIVE"
			BattleAction.SPECIAL:
				return "SPECIAL PROTOCOL INITIATED"
			_:
				return "PROCESSING..."
