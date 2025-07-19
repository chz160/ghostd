using Godot;

namespace Ghostd.Modules
{
	/// <summary>
	/// Core modules define the base stats and foundation of an AI
	/// </summary>
	public partial class CoreModule : ModuleBase
	{
		// Core-specific properties
		[Export] public int BaseHealth { get; set; } = 100;
		[Export] public int BaseProcessingPower { get; set; } = 10;
		[Export] public float StabilityRating { get; set; } = 1.0f;
		[Export] public string CoreArchitecture { get; set; } = "Standard";
		
		// Core modules always have ModuleType.Core
		public CoreModule()
		{
			Type = ModuleType.Core;
		}
		
		/// <summary>
		/// Calculate total health including modifiers and corruption
		/// </summary>
		public int GetTotalHealth()
		{
			float corruptionMultiplier = 1.0f - (CorruptionLevel * 0.2f); // Corruption reduces health
			return Mathf.Max(1, Mathf.RoundToInt(BaseHealth * corruptionMultiplier * StabilityRating));
		}
		
		/// <summary>
		/// Calculate total processing power
		/// </summary>
		public int GetTotalProcessingPower()
		{
			float corruptionBonus = 1.0f + (CorruptionLevel * 0.3f); // Corruption increases power
			return Mathf.RoundToInt(BaseProcessingPower * corruptionBonus);
		}
		
		public override string GetTerminalDisplay()
		{
			var display = base.GetTerminalDisplay();
			
			// Add core-specific stats
			display += "\nCORE SPECIFICATIONS:\n";
			display += $"  Health Pool: {GetTotalHealth()} HP\n";
			display += $"  Processing Power: {GetTotalProcessingPower()} PWR\n";
			display += $"  Stability: {StabilityRating:P0}\n";
			display += $"  Architecture: {CoreArchitecture}\n";
			
			return display;
		}
		
		/// <summary>
		/// Core modules can have special initialization sequences
		/// </summary>
		public virtual string GetBootSequence()
		{
			return $"Initializing {ModuleName}...\n" +
				   $"Loading {CoreArchitecture} architecture...\n" +
				   $"Stability check: {(StabilityRating >= 0.8f ? "PASSED" : "WARNING")}\n" +
				   $"Core online.";
		}
	}
}
