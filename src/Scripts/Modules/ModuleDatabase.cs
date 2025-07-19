using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Ghostd.Modules
{
	/// <summary>
	/// Singleton database that manages all modules in the game
	/// </summary>
	public partial class ModuleDatabase : Node
	{
		private static ModuleDatabase _instance;
		public static ModuleDatabase Instance
		{
			get
			{
				if (_instance == null)
				{
					GD.PrintErr("ModuleDatabase: Instance is null! Make sure it's added to the scene.");
				}
				return _instance;
			}
		}
		
		// Module collections
		private Dictionary<string, ModuleBase> _allModules = new Dictionary<string, ModuleBase>();
		private List<CoreModule> _coreModules = new List<CoreModule>();
		private List<BehaviorModule> _behaviorModules = new List<BehaviorModule>();
		private List<AugmentModule> _augmentModules = new List<AugmentModule>();
		
		// Module resource paths
		private const string ModuleResourcePath = "res://Resources/Modules/";
		
		public override void _EnterTree()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}
			_instance = this;
		}
		
		public override void _Ready()
		{
			// Make this node persistent
			ProcessMode = ProcessModeEnum.Always;
			
			// Load all modules
			LoadAllModules();
			
			GD.Print($"ModuleDatabase: Loaded {_allModules.Count} modules total");
			GD.Print($"  Cores: {_coreModules.Count}");
			GD.Print($"  Behaviors: {_behaviorModules.Count}");
			GD.Print($"  Augments: {_augmentModules.Count}");
		}
		
		/// <summary>
		/// Load all module resources from the file system
		/// </summary>
		private void LoadAllModules()
		{
			// Load Core modules
			LoadModulesOfType<CoreModule>("Cores/", _coreModules);
			
			// Load Behavior modules
			LoadModulesOfType<BehaviorModule>("Behaviors/", _behaviorModules);
			
			// Load Augment modules
			LoadModulesOfType<AugmentModule>("Augments/", _augmentModules);
		}
		
		/// <summary>
		/// Load modules of a specific type from a subdirectory
		/// </summary>
		private void LoadModulesOfType<T>(string subPath, List<T> targetList) where T : ModuleBase
		{
			var fullPath = ModuleResourcePath + subPath;
			var dir = DirAccess.Open(fullPath);
			
			if (dir == null)
			{
				GD.Print($"ModuleDatabase: Creating directory {fullPath}");
				DirAccess.MakeDirRecursiveAbsolute(fullPath);
				return;
			}
			
			dir.ListDirBegin();
			var fileName = dir.GetNext();
			
			while (!string.IsNullOrEmpty(fileName))
			{
				if (fileName.EndsWith(".tres") || fileName.EndsWith(".res"))
				{
					var resourcePath = fullPath + fileName;
					var resource = GD.Load<Resource>(resourcePath);
					
					if (resource is T module)
					{
						targetList.Add(module);
						_allModules[module.ModuleId] = module;
						GD.Print($"ModuleDatabase: Loaded {module.ModuleName} ({module.ModuleId})");
					}
				}
				
				fileName = dir.GetNext();
			}
			
			dir.ListDirEnd();
		}
		
		/// <summary>
		/// Get a module by its ID
		/// </summary>
		public ModuleBase GetModule(string moduleId)
		{
			return _allModules.TryGetValue(moduleId, out var module) ? module : null;
		}
		
		/// <summary>
		/// Get all modules of a specific type
		/// </summary>
		public List<T> GetModulesOfType<T>() where T : ModuleBase
		{
			if (typeof(T) == typeof(CoreModule))
				return _coreModules as List<T>;
			else if (typeof(T) == typeof(BehaviorModule))
				return _behaviorModules as List<T>;
			else if (typeof(T) == typeof(AugmentModule))
				return _augmentModules as List<T>;
			else
				return new List<T>();
		}
		
		/// <summary>
		/// Get all core modules
		/// </summary>
		public List<CoreModule> GetCoreModules() => new List<CoreModule>(_coreModules);
		
		/// <summary>
		/// Get all behavior modules
		/// </summary>
		public List<BehaviorModule> GetBehaviorModules() => new List<BehaviorModule>(_behaviorModules);
		
		/// <summary>
		/// Get all augment modules
		/// </summary>
		public List<AugmentModule> GetAugmentModules() => new List<AugmentModule>(_augmentModules);
		
		/// <summary>
		/// Get modules filtered by rarity
		/// </summary>
		public List<ModuleBase> GetModulesByRarity(ModuleRarity rarity)
		{
			return _allModules.Values.Where(m => m.Rarity == rarity).ToList();
		}
		
		/// <summary>
		/// Get modules filtered by tags
		/// </summary>
		public List<ModuleBase> GetModulesWithTag(string tag)
		{
			return _allModules.Values.Where(m => m.HasTag(tag)).ToList();
		}
		
		/// <summary>
		/// Get a random module of specific type and rarity
		/// </summary>
		public T GetRandomModule<T>(ModuleRarity? rarity = null) where T : ModuleBase
		{
			var modules = GetModulesOfType<T>();
			
			if (rarity.HasValue)
			{
				modules = modules.Where(m => m.Rarity == rarity.Value).ToList();
			}
			
			if (modules.Count == 0)
				return null;
				
			return modules[GD.RandRange(0, modules.Count - 1)];
		}
		
		/// <summary>
		/// Get a weighted random module based on rarity
		/// </summary>
		public ModuleBase GetWeightedRandomModule(ModuleType type)
		{
			// Rarity weights for drops
			var weights = new Dictionary<ModuleRarity, float>
			{
				{ ModuleRarity.Common, 0.50f },
				{ ModuleRarity.Uncommon, 0.30f },
				{ ModuleRarity.Rare, 0.15f },
				{ ModuleRarity.Epic, 0.04f },
				{ ModuleRarity.Legendary, 0.01f }
			};
			
			// Roll for rarity
			float roll = GD.Randf();
			float accumulated = 0f;
			ModuleRarity selectedRarity = ModuleRarity.Common;
			
			foreach (var kvp in weights)
			{
				accumulated += kvp.Value;
				if (roll <= accumulated)
				{
					selectedRarity = kvp.Key;
					break;
				}
			}
			
			// Get a module of the selected rarity
			return type switch
			{
				ModuleType.Core => GetRandomModule<CoreModule>(selectedRarity),
				ModuleType.Behavior => GetRandomModule<BehaviorModule>(selectedRarity),
				ModuleType.Augment => GetRandomModule<AugmentModule>(selectedRarity),
				_ => null
			};
		}
		
		/// <summary>
		/// Create a corrupted variant of an existing module
		/// </summary>
		public T CreateCorruptedVariant<T>(T baseModule) where T : ModuleBase
		{
			if (baseModule == null) return null;
			
			// Duplicate the module
			var corrupted = baseModule.Duplicate() as T;
			corrupted.ModuleId = baseModule.ModuleId + "_corrupted";
			corrupted.ModuleName = "Corrupted " + baseModule.ModuleName;
			corrupted.Rarity = ModuleRarity.Corrupted;
			corrupted.CorruptionLevel = (float)GD.RandRange(0.5f, 1.0f);
			
			// Boost stats but add instability
			corrupted.AttackModifier = Mathf.RoundToInt(corrupted.AttackModifier * 1.5f);
			corrupted.DefenseModifier = Mathf.RoundToInt(corrupted.DefenseModifier * 0.8f);
			corrupted.SpeedModifier = Mathf.RoundToInt(corrupted.SpeedModifier * 1.2f);
			
			// Add corrupted tag
			var tags = new List<string>(corrupted.Tags) { "corrupted", "unstable" };
			corrupted.Tags = tags.ToArray();
			
			return corrupted;
		}
		
		public override void _ExitTree()
		{
			if (_instance == this)
			{
				_instance = null;
			}
		}
	}
}