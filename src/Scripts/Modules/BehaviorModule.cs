using Godot;
using System.Collections.Generic;

namespace Ghostd.Modules
{
	/// <summary>
	/// Behavior modules define combat patterns and AI decision-making
	/// </summary>
	public partial class BehaviorModule : ModuleBase
	{
		// Behavior-specific properties
		[Export] public float AggressionLevel { get; set; } = 0.5f; // 0.0 = defensive, 1.0 = aggressive
		[Export] public float Adaptability { get; set; } = 0.5f; // How well it responds to enemy patterns
		[Export] public string PrimaryPattern { get; set; } = "Balanced";
		[Export] public string[] CombatRoutines { get; set; } = new string[0];
		
		// Behavior weights for different actions
		[Export] public float AttackWeight { get; set; } = 0.33f;
		[Export] public float DefendWeight { get; set; } = 0.33f;
		[Export] public float SpecialWeight { get; set; } = 0.34f;
		
		// Behavior modules always have ModuleType.Behavior
		public BehaviorModule()
		{
			Type = ModuleType.Behavior;
		}
		
		/// <summary>
		/// Get the next action based on behavior weights and current battle state
		/// </summary>
		public BattleAction GetNextAction(float healthPercent, float enemyHealthPercent, int turnNumber)
		{
			// Adjust weights based on battle state
			var adjustedAttackWeight = AttackWeight;
			var adjustedDefendWeight = DefendWeight;
			var adjustedSpecialWeight = SpecialWeight;
			
			// More aggressive when enemy is low health
			if (enemyHealthPercent < 0.3f)
			{
				adjustedAttackWeight *= (1 + AggressionLevel);
			}
			
			// More defensive when own health is low
			if (healthPercent < 0.3f)
			{
				adjustedDefendWeight *= (2 - AggressionLevel);
			}
			
			// Normalize weights
			float totalWeight = adjustedAttackWeight + adjustedDefendWeight + adjustedSpecialWeight;
			adjustedAttackWeight /= totalWeight;
			adjustedDefendWeight /= totalWeight;
			adjustedSpecialWeight /= totalWeight;
			
			// Random selection based on weights
			float random = GD.Randf();
			if (random < adjustedAttackWeight)
				return BattleAction.Attack;
			else if (random < adjustedAttackWeight + adjustedDefendWeight)
				return BattleAction.Defend;
			else
				return BattleAction.Special;
		}
		
		public override string GetTerminalDisplay()
		{
			var display = base.GetTerminalDisplay();
			
			// Add behavior-specific info
			display += "\nBEHAVIOR PROFILE:\n";
			display += $"  Pattern: {PrimaryPattern}\n";
			display += $"  Aggression: {GetAggressionBar()}\n";
			display += $"  Adaptability: {Adaptability:P0}\n";
			
			if (CombatRoutines.Length > 0)
			{
				display += "\n  Combat Routines:\n";
				foreach (var routine in CombatRoutines)
				{
					display += $"    - {routine}\n";
				}
			}
			
			return display;
		}
		
		private string GetAggressionBar()
		{
			int barLength = 10;
			int filledLength = Mathf.RoundToInt(AggressionLevel * barLength);
			string bar = "[";
			
			for (int i = 0; i < barLength; i++)
			{
				if (i < filledLength)
					bar += "█";
				else
					bar += "░";
			}
			
			bar += $"] {AggressionLevel:P0}";
			return bar;
		}
		
		/// <summary>
		/// Get a combat taunt/message based on behavior
		/// </summary>
		public string GetCombatMessage(BattleAction action)
		{
			if (AggressionLevel > 0.7f)
			{
				return action switch
				{
					BattleAction.Attack => "EXECUTING OFFENSIVE PROTOCOL",
					BattleAction.Defend => "TEMPORARY DEFENSIVE STANCE",
					BattleAction.Special => "UNLEASHING MAXIMUM FORCE",
					_ => "ANALYZING TARGET"
				};
			}
			else if (AggressionLevel < 0.3f)
			{
				return action switch
				{
					BattleAction.Attack => "CALCULATED STRIKE",
					BattleAction.Defend => "FORTIFYING DEFENSES",
					BattleAction.Special => "TACTICAL MANEUVER",
					_ => "EVALUATING OPTIONS"
				};
			}
			else
			{
				return action switch
				{
					BattleAction.Attack => "ENGAGING TARGET",
					BattleAction.Defend => "DEFENSIVE MEASURES ACTIVE",
					BattleAction.Special => "SPECIAL PROTOCOL INITIATED",
					_ => "PROCESSING..."
				};
			}
		}
	}
	
	public enum BattleAction
	{
		Attack,
		Defend,
		Special,
		Wait
	}
}
