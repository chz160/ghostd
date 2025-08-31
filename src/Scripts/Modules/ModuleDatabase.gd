extends Node

## Singleton database that manages all modules in the game

# Singleton instance
static var _instance: ModuleDatabase = null

# Module collections
var _all_modules: Dictionary = {}
var _core_modules: Array[CoreModule] = []
var _behavior_modules: Array[BehaviorModule] = []
var _augment_modules: Array[AugmentModule] = []

# Module resource paths
const MODULE_RESOURCE_PATH = "res://Resources/Modules/"

# Singleton instance getter
static func get_instance() -> ModuleDatabase:
	if _instance == null:
		push_error("ModuleDatabase: Instance is null! Make sure it's added to the scene.")
	return _instance

func _enter_tree():
	if _instance != null and _instance != self:
		queue_free()
		return
	_instance = self

func _ready():
	# Make this node persistent
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	# Load all modules
	load_all_modules()
	
	print("ModuleDatabase: Loaded %d modules total" % _all_modules.size())
	print("  Cores: %d" % _core_modules.size())
	print("  Behaviors: %d" % _behavior_modules.size())
	print("  Augments: %d" % _augment_modules.size())

## Load all module resources from the file system
func load_all_modules():
	# Load Core modules
	load_modules_of_type("Cores/", _core_modules)
	
	# Load Behavior modules
	load_modules_of_type("Behaviors/", _behavior_modules)
	
	# Load Augment modules
	load_modules_of_type("Augments/", _augment_modules)

## Load modules of a specific type from a subdirectory
func load_modules_of_type(sub_path: String, target_list: Array):
	var full_path = MODULE_RESOURCE_PATH + sub_path
	var dir = DirAccess.open(full_path)
	
	if dir == null:
		print("ModuleDatabase: Creating directory %s" % full_path)
		DirAccess.make_dir_recursive_absolute(full_path)
		return
	
	dir.list_dir_begin()
	var file_name = dir.get_next()
	
	while file_name != "":
		if file_name.ends_with(".tres") or file_name.ends_with(".res"):
			var resource_path = full_path + file_name
			var resource = load(resource_path)
			
			if resource is ModuleBase:
				target_list.append(resource)
				_all_modules[resource.module_id] = resource
				print("ModuleDatabase: Loaded %s (%s)" % [resource.module_name, resource.module_id])
		
		file_name = dir.get_next()
	
	dir.list_dir_end()

## Get a module by its ID
func get_module(module_id: String) -> ModuleBase:
	return _all_modules.get(module_id)

## Get all modules of a specific type
func get_modules_of_type(type: ModuleBase.ModuleType) -> Array:
	match type:
		ModuleBase.ModuleType.CORE:
			return _core_modules.duplicate()
		ModuleBase.ModuleType.BEHAVIOR:
			return _behavior_modules.duplicate()
		ModuleBase.ModuleType.AUGMENT:
			return _augment_modules.duplicate()
		_:
			return []

## Get all core modules
func get_core_modules() -> Array[CoreModule]:
	return _core_modules.duplicate()

## Get all behavior modules
func get_behavior_modules() -> Array[BehaviorModule]:
	return _behavior_modules.duplicate()

## Get all augment modules
func get_augment_modules() -> Array[AugmentModule]:
	return _augment_modules.duplicate()

## Get modules filtered by rarity
func get_modules_by_rarity(rarity: ModuleBase.ModuleRarity) -> Array[ModuleBase]:
	var result: Array[ModuleBase] = []
	for module in _all_modules.values():
		if module.rarity == rarity:
			result.append(module)
	return result

## Get modules filtered by tags
func get_modules_with_tag(tag: String) -> Array[ModuleBase]:
	var result: Array[ModuleBase] = []
	for module in _all_modules.values():
		if module.has_tag(tag):
			result.append(module)
	return result

## Get a random module of specific type and rarity
func get_random_module(type: ModuleBase.ModuleType, rarity: ModuleBase.ModuleRarity = -1) -> ModuleBase:
	var modules = get_modules_of_type(type)
	
	if rarity != -1:
		var filtered = []
		for module in modules:
			if module.rarity == rarity:
				filtered.append(module)
		modules = filtered
	
	if modules.is_empty():
		return null
	
	return modules[randi() % modules.size()]

## Get a weighted random module based on rarity
func get_weighted_random_module(type: ModuleBase.ModuleType) -> ModuleBase:
	# Rarity weights for drops
	var weights = {
		ModuleBase.ModuleRarity.COMMON: 0.50,
		ModuleBase.ModuleRarity.UNCOMMON: 0.30,
		ModuleBase.ModuleRarity.RARE: 0.15,
		ModuleBase.ModuleRarity.EPIC: 0.04,
		ModuleBase.ModuleRarity.LEGENDARY: 0.01
	}
	
	# Roll for rarity
	var roll = randf()
	var accumulated = 0.0
	var selected_rarity = ModuleBase.ModuleRarity.COMMON
	
	for rarity in weights:
		accumulated += weights[rarity]
		if roll <= accumulated:
			selected_rarity = rarity
			break
	
	# Get a module of the selected rarity
	return get_random_module(type, selected_rarity)

## Create a corrupted variant of an existing module
func create_corrupted_variant(base_module: ModuleBase) -> ModuleBase:
	if base_module == null:
		return null
	
	# Duplicate the module
	var corrupted = base_module.duplicate()
	corrupted.module_id = base_module.module_id + "_corrupted"
	corrupted.module_name = "Corrupted " + base_module.module_name
	corrupted.rarity = ModuleBase.ModuleRarity.CORRUPTED
	corrupted.corruption_level = randf_range(0.5, 1.0)
	
	# Boost stats but add instability
	corrupted.attack_modifier = roundi(corrupted.attack_modifier * 1.5)
	corrupted.defense_modifier = roundi(corrupted.defense_modifier * 0.8)
	corrupted.speed_modifier = roundi(corrupted.speed_modifier * 1.2)
	
	# Add corrupted tag
	var tags = corrupted.tags.duplicate()
	tags.append("corrupted")
	tags.append("unstable")
	corrupted.tags = tags
	
	return corrupted

func _exit_tree():
	if _instance == self:
		_instance = null
