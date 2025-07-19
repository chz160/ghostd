using Godot;
using Ghostd.Modules;
using System.Collections.Generic;

public partial class AIAssembly : Control
{
	// UI References
	private RichTextLabel _assemblyDisplay;
	private RichTextLabel _statsDisplay;
	private Button _coreSlotButton;
	private Button _behaviorSlotButton;
	private Button _augmentSlotButton;
	private Button _assembleButton;
	private Button _clearButton;
	private Label _statusLabel;
	private Button _backButton;
	
	// Module Selection
	private Panel _moduleSelectionPanel;
	private ItemList _moduleList;
	private RichTextLabel _modulePreview;
	private Button _selectButton;
	private Button _cancelButton;
	
	// Current Assembly
	private CoreModule _selectedCore;
	private BehaviorModule _selectedBehavior;
	private AugmentModule _selectedAugment;
	private ModuleType _selectingType;
	
	// Assembled AI stats
	private Dictionary<string, int> _assembledStats = new Dictionary<string, int>();
	
	public override void _Ready()
	{
		// Get UI references
		_assemblyDisplay = GetNode<RichTextLabel>("Container/DisplayContainer/AssemblyPanel/AssemblyDisplay");
		_statsDisplay = GetNode<RichTextLabel>("Container/DisplayContainer/StatsPanel/StatsDisplay");
		_coreSlotButton = GetNode<Button>("Container/ModuleSlots/CoreSlot");
		_behaviorSlotButton = GetNode<Button>("Container/ModuleSlots/BehaviorSlot");
		_augmentSlotButton = GetNode<Button>("Container/ModuleSlots/AugmentSlot");
		_assembleButton = GetNode<Button>("Container/Actions/AssembleButton");
		_clearButton = GetNode<Button>("Container/Actions/ClearButton");
		_statusLabel = GetNode<Label>("Container/StatusLabel");
		_backButton = GetNode<Button>("Container/BackButton");
		
		// Module selection panel
		_moduleSelectionPanel = GetNode<Panel>("ModuleSelectionPanel");
		_moduleList = GetNode<ItemList>("ModuleSelectionPanel/VBox/ModuleList");
		_modulePreview = GetNode<RichTextLabel>("ModuleSelectionPanel/VBox/ModulePreview");
		_selectButton = GetNode<Button>("ModuleSelectionPanel/VBox/Actions/SelectButton");
		_cancelButton = GetNode<Button>("ModuleSelectionPanel/VBox/Actions/CancelButton");
		
		// Connect buttons
		_coreSlotButton.Pressed += () => OpenModuleSelection(ModuleType.Core);
		_behaviorSlotButton.Pressed += () => OpenModuleSelection(ModuleType.Behavior);
		_augmentSlotButton.Pressed += () => OpenModuleSelection(ModuleType.Augment);
		_assembleButton.Pressed += OnAssemblePressed;
		_clearButton.Pressed += OnClearPressed;
		_selectButton.Pressed += OnSelectModulePressed;
		_cancelButton.Pressed += OnCancelSelectionPressed;
		_moduleList.ItemSelected += OnModuleListItemSelected;
		_backButton.Pressed += ReturnToMainTerminal;
		
		// Initialize
		_moduleSelectionPanel.Visible = false;
		UpdateDisplay();
	}
	
	private void OpenModuleSelection(ModuleType type)
	{
		_selectingType = type;
		_moduleSelectionPanel.Visible = true;
		_moduleList.Clear();
		_modulePreview.Clear();
		
		// Populate module list
		List<ModuleBase> modules = type switch
		{
			ModuleType.Core => new List<ModuleBase>(ModuleDatabase.Instance.GetCoreModules()),
			ModuleType.Behavior => new List<ModuleBase>(ModuleDatabase.Instance.GetBehaviorModules()),
			ModuleType.Augment => new List<ModuleBase>(ModuleDatabase.Instance.GetAugmentModules()),
			_ => new List<ModuleBase>()
		};
		
		foreach (var module in modules)
		{
			var itemText = $"{module.ModuleName} [{module.Rarity}]";
			_moduleList.AddItem(itemText);
			
			// Store module reference in metadata
			_moduleList.SetItemMetadata(_moduleList.GetItemCount() - 1, module);
			
			// Color based on rarity
			var color = module.GetRarityColor();
			_moduleList.SetItemCustomFgColor(_moduleList.GetItemCount() - 1, color);
		}
		
		_statusLabel.Text = $"Select a {type} module...";
	}
	
	private void OnModuleListItemSelected(long index)
	{
		var module = _moduleList.GetItemMetadata((int)index).As<ModuleBase>();
		if (module != null)
		{
			_modulePreview.Clear();
			_modulePreview.AppendText(module.GetTerminalDisplay());
		}
	}
	
	private void OnSelectModulePressed()
	{
		var selectedItems = _moduleList.GetSelectedItems();
		if (selectedItems.Length == 0) return;
		
		var module = _moduleList.GetItemMetadata(selectedItems[0]).As<ModuleBase>();
		if (module == null) return;
		
		// Assign to appropriate slot
		switch (_selectingType)
		{
			case ModuleType.Core:
				_selectedCore = module as CoreModule;
				break;
			case ModuleType.Behavior:
				_selectedBehavior = module as BehaviorModule;
				break;
			case ModuleType.Augment:
				_selectedAugment = module as AugmentModule;
				break;
		}
		
		_moduleSelectionPanel.Visible = false;
		UpdateDisplay();
		UpdateSlotButtons();
	}
	
	private void OnCancelSelectionPressed()
	{
		_moduleSelectionPanel.Visible = false;
		_statusLabel.Text = "Module selection cancelled.";
	}
	
	private void UpdateSlotButtons()
	{
		// Update button text to show selected modules
		_coreSlotButton.Text = _selectedCore != null 
			? $"CORE: {_selectedCore.ModuleName}" 
			: "CORE: [EMPTY]";
			
		_behaviorSlotButton.Text = _selectedBehavior != null 
			? $"BEHAVIOR: {_selectedBehavior.ModuleName}" 
			: "BEHAVIOR: [EMPTY]";
			
		_augmentSlotButton.Text = _selectedAugment != null 
			? $"AUGMENT: {_selectedAugment.ModuleName}" 
			: "AUGMENT: [EMPTY]";
			
		// Enable assemble button if all slots filled
		_assembleButton.Disabled = _selectedCore == null || _selectedBehavior == null || _selectedAugment == null;
	}
	
	private void UpdateDisplay()
	{
		UpdateSlotButtons();
		UpdateAssemblyDisplay();
		UpdateStatsDisplay();
	}
	
	private void UpdateAssemblyDisplay()
	{
		_assemblyDisplay.Clear();
		
		_assemblyDisplay.AppendText("[color=#00ff00]═══ AI ASSEMBLY ═══[/color]\n\n");
		
		// Core
		_assemblyDisplay.AppendText("[color=#00ffff]CORE MODULE:[/color]\n");
		if (_selectedCore != null)
		{
			_assemblyDisplay.AppendText($"  {_selectedCore.ModuleName}\n");
			_assemblyDisplay.AppendText($"  Architecture: {_selectedCore.CoreArchitecture}\n");
			_assemblyDisplay.AppendText($"  Base HP: {_selectedCore.GetTotalHealth()}\n");
			_assemblyDisplay.AppendText($"  Base PWR: {_selectedCore.GetTotalProcessingPower()}\n");
		}
		else
		{
			_assemblyDisplay.AppendText("  [color=#666666][EMPTY SLOT][/color]\n");
		}
		
		_assemblyDisplay.AppendText("\n");
		
		// Behavior
		_assemblyDisplay.AppendText("[color=#00ffff]BEHAVIOR MODULE:[/color]\n");
		if (_selectedBehavior != null)
		{
			_assemblyDisplay.AppendText($"  {_selectedBehavior.ModuleName}\n");
			_assemblyDisplay.AppendText($"  Pattern: {_selectedBehavior.PrimaryPattern}\n");
			_assemblyDisplay.AppendText($"  Aggression: {_selectedBehavior.AggressionLevel:P0}\n");
		}
		else
		{
			_assemblyDisplay.AppendText("  [color=#666666][EMPTY SLOT][/color]\n");
		}
		
		_assemblyDisplay.AppendText("\n");
		
		// Augment
		_assemblyDisplay.AppendText("[color=#00ffff]AUGMENT MODULE:[/color]\n");
		if (_selectedAugment != null)
		{
			_assemblyDisplay.AppendText($"  {_selectedAugment.ModuleName}\n");
			_assemblyDisplay.AppendText($"  Ability: {_selectedAugment.AbilityName}\n");
			_assemblyDisplay.AppendText($"  Cost: {_selectedAugment.PowerCost} PWR\n");
		}
		else
		{
			_assemblyDisplay.AppendText("  [color=#666666][EMPTY SLOT][/color]\n");
		}
		
		// ASCII art preview
		if (_selectedCore != null && _selectedBehavior != null && _selectedAugment != null)
		{
			_assemblyDisplay.AppendText("\n[color=#00ff00]═══ AI PREVIEW ═══[/color]\n");
			_assemblyDisplay.AppendText(GenerateAIAsciiArt());
		}
	}
	
	private void UpdateStatsDisplay()
	{
		_statsDisplay.Clear();
		
		_statsDisplay.AppendText("[color=#00ff00]═══ COMBINED STATS ═══[/color]\n\n");
		
		if (_selectedCore == null || _selectedBehavior == null || _selectedAugment == null)
		{
			_statsDisplay.AppendText("[color=#666666]Select all modules to see combined stats[/color]\n");
			return;
		}
		
		// Calculate combined stats
		CalculateCombinedStats();
		
		// Display stats
		_statsDisplay.AppendText($"[color=#ff6666]HEALTH:[/color] {_assembledStats["HP"]}\n");
		_statsDisplay.AppendText($"[color=#6666ff]POWER:[/color] {_assembledStats["PWR"]}\n");
		_statsDisplay.AppendText($"[color=#ff9966]ATTACK:[/color] {_assembledStats["ATK"]}\n");
		_statsDisplay.AppendText($"[color=#66ff66]DEFENSE:[/color] {_assembledStats["DEF"]}\n");
		_statsDisplay.AppendText($"[color=#ffff66]SPEED:[/color] {_assembledStats["SPD"]}\n");
		
		// Stability and corruption
		float totalCorruption = _selectedCore.CorruptionLevel + _selectedBehavior.CorruptionLevel + _selectedAugment.CorruptionLevel;
		if (totalCorruption > 0)
		{
			_statsDisplay.AppendText($"\n[color=#ff4444]CORRUPTION:[/color] {totalCorruption:P0}\n");
		}
		
		_statsDisplay.AppendText($"\n[color=#00ffff]STABILITY:[/color] {_selectedCore.StabilityRating:P0}\n");
		
		// Special properties
		_statsDisplay.AppendText("\n[color=#00ff00]═══ PROPERTIES ═══[/color]\n");
		
		var allTags = new List<string>();
		allTags.AddRange(_selectedCore.Tags);
		allTags.AddRange(_selectedBehavior.Tags);
		allTags.AddRange(_selectedAugment.Tags);
		
		foreach (var tag in allTags)
		{
			_statsDisplay.AppendText($"• {tag}\n");
		}
	}
	
	private void CalculateCombinedStats()
	{
		_assembledStats.Clear();
		
		// Base stats from core
		_assembledStats["HP"] = _selectedCore.GetTotalHealth();
		_assembledStats["PWR"] = _selectedCore.GetTotalProcessingPower();
		
		// Combine modifiers from all modules
		_assembledStats["ATK"] = 10 + _selectedCore.AttackModifier + _selectedBehavior.AttackModifier + _selectedAugment.AttackModifier;
		_assembledStats["DEF"] = 10 + _selectedCore.DefenseModifier + _selectedBehavior.DefenseModifier + _selectedAugment.DefenseModifier;
		_assembledStats["SPD"] = 10 + _selectedCore.SpeedModifier + _selectedBehavior.SpeedModifier + _selectedAugment.SpeedModifier;
		
		// Apply behavior modifiers
		if (_selectedBehavior.AggressionLevel > 0.7f)
		{
			_assembledStats["ATK"] = Mathf.RoundToInt(_assembledStats["ATK"] * 1.2f);
		}
		else if (_selectedBehavior.AggressionLevel < 0.3f)
		{
			_assembledStats["DEF"] = Mathf.RoundToInt(_assembledStats["DEF"] * 1.2f);
		}
		
		// Ensure minimum values
		foreach (var key in _assembledStats.Keys)
		{
			if (_assembledStats[key] < 1)
				_assembledStats[key] = 1;
		}
	}
	
	private string GenerateAIAsciiArt()
	{
		// Combine ASCII art from modules creatively
		return @"
    ╔═══════╗
    ║  ◊-◊  ║
    ║ ┌───┐ ║
    ║ │ AI│ ║
    ║ └───┘ ║
    ╚═══════╝
";
	}
	
	private void OnAssemblePressed()
	{
		if (_selectedCore == null || _selectedBehavior == null || _selectedAugment == null)
		{
			_statusLabel.Text = "ERROR: All module slots must be filled!";
			return;
		}
		
		_statusLabel.Text = "AI ASSEMBLED SUCCESSFULLY! Ready for deployment.";
		
		// TODO: Save assembled AI configuration
		// TODO: Transition to battle or main menu
	}
	
	private void OnClearPressed()
	{
		_selectedCore = null;
		_selectedBehavior = null;
		_selectedAugment = null;
		UpdateDisplay();
		_statusLabel.Text = "Assembly cleared. Select new modules.";
	}
	
	// Input handling for escape key
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				ReturnToMainTerminal();
			}
		}
	}
	
	private void ReturnToMainTerminal()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainTerminal.tscn");
	}
}