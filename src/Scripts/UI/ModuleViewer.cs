using Godot;
using Ghostd.Modules;
using System.Collections.Generic;

public partial class ModuleViewer : Control
{
	private RichTextLabel _moduleDisplay;
	private Button _previousButton;
	private Button _nextButton;
	private Label _moduleTypeLabel;
	private Label _moduleCountLabel;
	private Button _coreButton;
	private Button _behaviorButton;
	private Button _augmentButton;
	private Button _backButton;
	
	private List<ModuleBase> _allModules = new List<ModuleBase>();
	private int _currentIndex = 0;
	private ModuleType _currentType = ModuleType.Core;
	
	public override void _Ready()
	{
		// Get node references
		_moduleDisplay = GetNode<RichTextLabel>("Container/ScrollContainer/ModuleDisplay");
		_previousButton = GetNode<Button>("Container/Navigation/PreviousButton");
		_nextButton = GetNode<Button>("Container/Navigation/NextButton");
		_moduleTypeLabel = GetNode<Label>("Container/ModuleInfo/ModuleTypeLabel");
		_moduleCountLabel = GetNode<Label>("Container/ModuleInfo/ModuleCountLabel");
		_coreButton = GetNode<Button>("Container/TypeButtons/CoreButton");
		_behaviorButton = GetNode<Button>("Container/TypeButtons/BehaviorButton");
		_augmentButton = GetNode<Button>("Container/TypeButtons/AugmentButton");
		_backButton = GetNode<Button>("Container/BackButton");
		
		// Connect buttons
		if (_previousButton != null)
			_previousButton.Pressed += OnPreviousPressed;
		if (_nextButton != null)
			_nextButton.Pressed += OnNextPressed;
		if (_coreButton != null)
			_coreButton.Pressed += () => SwitchToType(ModuleType.Core);
		if (_behaviorButton != null)
			_behaviorButton.Pressed += () => SwitchToType(ModuleType.Behavior);
		if (_augmentButton != null)
			_augmentButton.Pressed += () => SwitchToType(ModuleType.Augment);
		if (_backButton != null)
			_backButton.Pressed += ReturnToMainTerminal;
			
		// Load modules after a short delay to ensure ModuleDatabase is ready
		GetTree().CreateTimer(0.1f).Timeout += LoadModules;
	}
	
	private void LoadModules()
	{
		if (ModuleDatabase.Instance == null)
		{
			GD.PrintErr("ModuleViewer: ModuleDatabase not ready!");
			return;
		}
		
		// Load all modules
		RefreshModuleList();
		DisplayCurrentModule();
	}
	
	private void RefreshModuleList()
	{
		_allModules.Clear();
		
		switch (_currentType)
		{
			case ModuleType.Core:
				_allModules.AddRange(ModuleDatabase.Instance.GetCoreModules());
				break;
			case ModuleType.Behavior:
				_allModules.AddRange(ModuleDatabase.Instance.GetBehaviorModules());
				break;
			case ModuleType.Augment:
				_allModules.AddRange(ModuleDatabase.Instance.GetAugmentModules());
				break;
		}
		
		_currentIndex = 0;
		UpdateLabels();
	}
	
	private void UpdateLabels()
	{
		if (_moduleTypeLabel != null)
			_moduleTypeLabel.Text = $"TYPE: {_currentType.ToString().ToUpper()}";
			
		if (_moduleCountLabel != null && _allModules.Count > 0)
			_moduleCountLabel.Text = $"[{_currentIndex + 1}/{_allModules.Count}]";
		else if (_moduleCountLabel != null)
			_moduleCountLabel.Text = "[0/0]";
	}
	
	private void DisplayCurrentModule()
	{
		if (_moduleDisplay == null) return;
		
		_moduleDisplay.Clear();
		
		if (_allModules.Count == 0)
		{
			_moduleDisplay.AppendText("[color=#ff4444]NO MODULES FOUND[/color]\n");
			_moduleDisplay.AppendText($"Module type: {_currentType}\n");
			return;
		}
		
		if (_currentIndex >= 0 && _currentIndex < _allModules.Count)
		{
			var module = _allModules[_currentIndex];
			_moduleDisplay.AppendText(module.GetTerminalDisplay());
			
			// Add type-specific information
			switch (module)
			{
				case CoreModule core:
					DisplayCoreSpecifics(core);
					break;
				case BehaviorModule behavior:
					DisplayBehaviorSpecifics(behavior);
					break;
				case AugmentModule augment:
					DisplayAugmentSpecifics(augment);
					break;
			}
		}
		
		UpdateLabels();
	}
	
	private void DisplayCoreSpecifics(CoreModule core)
	{
		_moduleDisplay.AppendText("\n[color=#00ff00]BOOT SEQUENCE:[/color]\n");
		_moduleDisplay.AppendText(core.GetBootSequence());
	}
	
	private void DisplayBehaviorSpecifics(BehaviorModule behavior)
	{
		_moduleDisplay.AppendText("\n[color=#00ff00]COMBAT PREVIEW:[/color]\n");
		_moduleDisplay.AppendText($"Attack Action: {behavior.GetCombatMessage(BattleAction.Attack)}\n");
		_moduleDisplay.AppendText($"Defend Action: {behavior.GetCombatMessage(BattleAction.Defend)}\n");
		_moduleDisplay.AppendText($"Special Action: {behavior.GetCombatMessage(BattleAction.Special)}\n");
	}
	
	private void DisplayAugmentSpecifics(AugmentModule augment)
	{
		_moduleDisplay.AppendText("\n[color=#00ff00]ACTIVATION PREVIEW:[/color]\n");
		_moduleDisplay.AppendText(augment.GetActivationLog("TEST_AI", "ENEMY_AI"));
	}
	
	private void OnPreviousPressed()
	{
		if (_allModules.Count == 0) return;
		
		_currentIndex--;
		if (_currentIndex < 0)
			_currentIndex = _allModules.Count - 1;
			
		DisplayCurrentModule();
	}
	
	private void OnNextPressed()
	{
		if (_allModules.Count == 0) return;
		
		_currentIndex++;
		if (_currentIndex >= _allModules.Count)
			_currentIndex = 0;
			
		DisplayCurrentModule();
	}
	
	public void SwitchToType(ModuleType type)
	{
		_currentType = type;
		RefreshModuleList();
		DisplayCurrentModule();
	}
	
	// Input handling for keyboard navigation
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Left:
				case Key.A:
					OnPreviousPressed();
					break;
				case Key.Right:
				case Key.D:
					OnNextPressed();
					break;
				case Key.Key1:
					SwitchToType(ModuleType.Core);
					break;
				case Key.Key2:
					SwitchToType(ModuleType.Behavior);
					break;
				case Key.Key3:
					SwitchToType(ModuleType.Augment);
					break;
				case Key.Escape:
					ReturnToMainTerminal();
					break;
			}
		}
	}
	
	private void ReturnToMainTerminal()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainTerminal.tscn");
	}
}