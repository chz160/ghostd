using Godot;
using System.Collections.Generic;

namespace Ghostd.Modules
{
	/// <summary>
	/// Base class for all AI modules in the game
	/// </summary>
	public abstract partial class ModuleBase : Resource
	{
		[Export] public string ModuleId { get; set; } = "";
		[Export] public string ModuleName { get; set; } = "Unknown Module";
		[Export] public string Description { get; set; } = "";
		[Export] public ModuleType Type { get; set; }
		[Export] public ModuleRarity Rarity { get; set; } = ModuleRarity.Common;
		
		// Base stats that all modules can modify
		[Export] public int AttackModifier { get; set; } = 0;
		[Export] public int DefenseModifier { get; set; } = 0;
		[Export] public int SpeedModifier { get; set; } = 0;
		[Export] public int PowerModifier { get; set; } = 0;
		
		// ASCII art representation (for terminal display)
		[Export(PropertyHint.MultilineText)] public string AsciiArt { get; set; } = "";
		
		// Flavor text for terminal display
		[Export(PropertyHint.MultilineText)] public string FlavorText { get; set; } = "";
		
		// Corruption level (affects stats and behavior)
		[Export] public float CorruptionLevel { get; set; } = 0.0f;
		
		// Tags for special properties
		[Export] public string[] Tags { get; set; } = new string[0];
		
		/// <summary>
		/// Get the display color for this module based on rarity
		/// </summary>
		public Color GetRarityColor()
		{
			return Rarity switch
			{
				ModuleRarity.Common => new Color(0.7f, 0.7f, 0.7f), // Gray
				ModuleRarity.Uncommon => new Color(0.2f, 1.0f, 0.2f), // Green
				ModuleRarity.Rare => new Color(0.2f, 0.6f, 1.0f), // Blue
				ModuleRarity.Epic => new Color(0.8f, 0.2f, 1.0f), // Purple
				ModuleRarity.Legendary => new Color(1.0f, 0.8f, 0.2f), // Gold
				ModuleRarity.Corrupted => new Color(1.0f, 0.2f, 0.2f), // Red
				_ => new Color(1.0f, 1.0f, 1.0f) // White
			};
		}
		
		/// <summary>
		/// Get formatted terminal display text for this module
		/// </summary>
		public virtual string GetTerminalDisplay()
		{
			var color = GetRarityColor();
			var hexColor = $"#{(int)(color.R * 255):X2}{(int)(color.G * 255):X2}{(int)(color.B * 255):X2}";
			
			var display = $"[color={hexColor}]══════════════════════════════════════[/color]\n";
			display += $"[color={hexColor}]MODULE: {ModuleName.ToUpper()}[/color]\n";
			display += $"[color={hexColor}]TYPE: {Type} | RARITY: {Rarity}[/color]\n";
			display += $"[color={hexColor}]══════════════════════════════════════[/color]\n";
			
			if (!string.IsNullOrEmpty(AsciiArt))
			{
				display += $"{AsciiArt}\n";
			}
			
			display += $"\n{Description}\n\n";
			
			// Show stat modifiers
			display += "STAT MODIFIERS:\n";
			if (AttackModifier != 0) display += $"  ATK: {(AttackModifier > 0 ? "+" : "")}{AttackModifier}\n";
			if (DefenseModifier != 0) display += $"  DEF: {(DefenseModifier > 0 ? "+" : "")}{DefenseModifier}\n";
			if (SpeedModifier != 0) display += $"  SPD: {(SpeedModifier > 0 ? "+" : "")}{SpeedModifier}\n";
			if (PowerModifier != 0) display += $"  PWR: {(PowerModifier > 0 ? "+" : "")}{PowerModifier}\n";
			
			if (CorruptionLevel > 0)
			{
				display += $"\n[color=#ff4444]CORRUPTION: {CorruptionLevel:P0}[/color]\n";
			}
			
			if (!string.IsNullOrEmpty(FlavorText))
			{
				display += $"\n[i]{FlavorText}[/i]\n";
			}
			
			return display;
		}
		
		/// <summary>
		/// Apply corruption effects to the module
		/// </summary>
		public virtual void ApplyCorruption(float amount)
		{
			CorruptionLevel = Mathf.Clamp(CorruptionLevel + amount, 0.0f, 1.0f);
			
			// Corruption can boost stats but at a cost
			if (CorruptionLevel > 0.5f)
			{
				AttackModifier = Mathf.RoundToInt(AttackModifier * (1 + CorruptionLevel));
				// Add instability or other negative effects
			}
		}
		
		/// <summary>
		/// Check if this module has a specific tag
		/// </summary>
		public bool HasTag(string tag)
		{
			foreach (var t in Tags)
			{
				if (t.Equals(tag, System.StringComparison.OrdinalIgnoreCase))
					return true;
			}
			return false;
		}
	}
	
	/// <summary>
	/// Module type enumeration
	/// </summary>
	public enum ModuleType
	{
		Core,
		Behavior,
		Augment
	}
	
	/// <summary>
	/// Module rarity levels
	/// </summary>
	public enum ModuleRarity
	{
		Common,
		Uncommon,
		Rare,
		Epic,
		Legendary,
		Corrupted
	}
}