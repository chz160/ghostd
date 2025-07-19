using Godot;
using System.Collections.Generic;

namespace Ghostd.Modules
{
	/// <summary>
	/// Augment modules provide special abilities and unique effects
	/// </summary>
	public partial class AugmentModule : ModuleBase
	{
		// Augment-specific properties
		[Export] public string AbilityName { get; set; } = "Unknown Ability";
		[Export] public string AbilityDescription { get; set; } = "";
		[Export] public float CooldownTurns { get; set; } = 3;
		[Export] public float PowerCost { get; set; } = 20;
		[Export] public TargetType TargetingMode { get; set; } = TargetType.Enemy;
		
		// Effect properties
		[Export] public float EffectPower { get; set; } = 1.0f;
		[Export] public float EffectDuration { get; set; } = 0; // 0 = instant
		[Export] public string[] EffectTags { get; set; } = new string[0];
		
		// Activation conditions
		[Export] public string ActivationCondition { get; set; } = "Manual";
		[Export] public float AutoTriggerThreshold { get; set; } = 0.0f;
		
		// Visual/Audio feedback
		[Export] public string ActivationMessage { get; set; } = "AUGMENT ACTIVATED";
		[Export] public string EffectAnimation { get; set; } = "default";
		
		// Augment modules always have ModuleType.Augment
		public AugmentModule()
		{
			Type = ModuleType.Augment;
		}
		
		/// <summary>
		/// Check if the augment can be activated
		/// </summary>
		public bool CanActivate(int currentPower, int currentCooldown, float healthPercent = 1.0f)
		{
			// Check power cost
			if (currentPower < PowerCost)
				return false;
				
			// Check cooldown
			if (currentCooldown > 0)
				return false;
				
			// Check auto-trigger conditions
			if (ActivationCondition == "LowHealth" && healthPercent > AutoTriggerThreshold)
				return false;
				
			return true;
		}
		
		/// <summary>
		/// Calculate the actual effect value considering corruption and other modifiers
		/// </summary>
		public float GetEffectValue()
		{
			// Corruption can enhance augment effects but make them unstable
			float corruptionBonus = 1.0f + (CorruptionLevel * 0.5f);
			float instability = CorruptionLevel > 0.5f ? GD.Randf() * 0.2f - 0.1f : 0f;
			
			return EffectPower * corruptionBonus * (1.0f + instability);
		}
		
		public override string GetTerminalDisplay()
		{
			var display = base.GetTerminalDisplay();
			
			// Add augment-specific info
			display += "\nAUGMENT SPECIFICATIONS:\n";
			display += $"  Ability: {AbilityName}\n";
			display += $"  {AbilityDescription}\n\n";
			
			display += "  PARAMETERS:\n";
			display += $"    Power Cost: {PowerCost} PWR\n";
			display += $"    Cooldown: {CooldownTurns} turns\n";
			display += $"    Target: {TargetingMode}\n";
			display += $"    Effect Power: {GetEffectValue():F1}\n";
			
			if (EffectDuration > 0)
			{
				display += $"    Duration: {EffectDuration} turns\n";
			}
			
			if (ActivationCondition != "Manual")
			{
				display += $"\n  AUTO-TRIGGER: {ActivationCondition}";
				if (AutoTriggerThreshold > 0)
				{
					display += $" (< {AutoTriggerThreshold:P0})";
				}
				display += "\n";
			}
			
			if (EffectTags.Length > 0)
			{
				display += "\n  EFFECT TAGS: ";
				display += string.Join(", ", EffectTags);
				display += "\n";
			}
			
			return display;
		}
		
		/// <summary>
		/// Get the activation message for the battle log
		/// </summary>
		public string GetActivationLog(string userName, string targetName)
		{
			var log = $"[color=#00ffff]{userName} ACTIVATES {AbilityName.ToUpper()}![/color]\n";
			log += $"> {ActivationMessage}\n";
			
			if (TargetingMode == TargetType.Enemy)
			{
				log += $"> TARGET: {targetName}\n";
			}
			else if (TargetingMode == TargetType.Self)
			{
				log += $"> SELF-TARGETED PROTOCOL\n";
			}
			
			// Add effect description
			foreach (var tag in EffectTags)
			{
				log += GetEffectDescription(tag);
			}
			
			return log;
		}
		
		private string GetEffectDescription(string effectTag)
		{
			return effectTag.ToLower() switch
			{
				"damage" => $"> DAMAGE OUTPUT: {GetEffectValue():F0}\n",
				"heal" => $"> REPAIR SYSTEMS: +{GetEffectValue():F0} HP\n",
				"shield" => $"> DEFENSIVE MATRIX: +{GetEffectValue():F0} DEF\n",
				"drain" => $"> ENERGY DRAIN: {GetEffectValue():F0}\n",
				"stun" => $"> SYSTEM DISRUPTION: {EffectDuration} CYCLES\n",
				"boost" => $"> PERFORMANCE BOOST: +{GetEffectValue():F0}%\n",
				_ => ""
			};
		}
		
		/// <summary>
		/// Apply the augment's effect
		/// </summary>
		public virtual AugmentEffect ApplyEffect(IAIBattler user, IAIBattler target)
		{
			var effect = new AugmentEffect
			{
				SourceAugment = this,
				Value = GetEffectValue(),
				Duration = EffectDuration,
				Tags = EffectTags
			};
			
			// Apply immediate effects based on tags
			foreach (var tag in EffectTags)
			{
				ApplyEffectByTag(tag, effect, user, target);
			}
			
			return effect;
		}
		
		private void ApplyEffectByTag(string tag, AugmentEffect effect, IAIBattler user, IAIBattler target)
		{
			switch (tag.ToLower())
			{
				case "damage":
					if (TargetingMode == TargetType.Enemy)
						effect.DamageDealt = Mathf.RoundToInt(effect.Value);
					break;
					
				case "heal":
					if (TargetingMode == TargetType.Self)
						effect.HealingDone = Mathf.RoundToInt(effect.Value);
					break;
					
				case "shield":
					effect.ShieldAmount = Mathf.RoundToInt(effect.Value);
					break;
					
				case "boost":
					effect.StatModifiers["ATK"] = effect.Value * 0.01f;
					break;
			}
		}
	}
	
	public enum TargetType
	{
		Self,
		Enemy,
		All,
		Random
	}
	
	/// <summary>
	/// Represents the effect of an augment activation
	/// </summary>
	public class AugmentEffect
	{
		public AugmentModule SourceAugment { get; set; }
		public float Value { get; set; }
		public float Duration { get; set; }
		public string[] Tags { get; set; }
		
		// Effect results
		public int DamageDealt { get; set; }
		public int HealingDone { get; set; }
		public int ShieldAmount { get; set; }
		public Dictionary<string, float> StatModifiers { get; set; } = new Dictionary<string, float>();
		
		public bool IsExpired(int currentTurn, int appliedTurn)
		{
			return Duration > 0 && (currentTurn - appliedTurn) >= Duration;
		}
	}
	
	/// <summary>
	/// Interface for AI battlers that can be targeted by augments
	/// </summary>
	public interface IAIBattler
	{
		int CurrentHealth { get; set; }
		int MaxHealth { get; }
		int CurrentPower { get; set; }
		Dictionary<string, float> Stats { get; }
		void ApplyEffect(AugmentEffect effect);
	}
}
