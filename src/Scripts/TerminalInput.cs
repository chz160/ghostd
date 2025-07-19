using Godot;
using System.Collections.Generic;

public partial class TerminalInput : Control
{
	private Terminal _terminal;
	private Button _tapDetector;
	private GameState _currentState = GameState.Startup;
	
	private readonly List<MenuOption> _mainMenuOptions = new List<MenuOption>
	{
		new MenuOption("1", "NEW BATTLE", GameState.PreBattle),
		new MenuOption("2", "MODULE LAB", GameState.ModuleLab),
		new MenuOption("3", "ARCHIVES", GameState.Archives),
		new MenuOption("4", "SYSTEM", GameState.System)
	};
	
	public override void _Ready()
	{
		// Get the Terminal node which is a sibling of InputArea's parent
		_terminal = GetNode<Terminal>("/root/MainTerminal/Terminal");
		_tapDetector = GetNode<Button>("TapDetector");
		
		if (_terminal == null)
		{
			GD.PrintErr("TerminalInput: Failed to find Terminal node!");
			return;
		}
		
		_tapDetector.Pressed += OnTapDetected;
		
		// Set responsive input area height
		UpdateInputAreaSize();
		
		// Connect to viewport size changes
		GetViewport().SizeChanged += OnViewportSizeChanged;
		
		// Don't show menu on startup - let the terminal show its startup sequence first
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (_currentState == GameState.MainMenu)
			{
				switch (keyEvent.Keycode)
				{
					case Key.Key1:
						HandleMenuSelection(0);
						break;
					case Key.Key2:
						HandleMenuSelection(1);
						break;
					case Key.Key3:
						HandleMenuSelection(2);
						break;
					case Key.Key4:
						HandleMenuSelection(3);
						break;
				}
			}
			else if (_currentState == GameState.ModuleLabMenu)
			{
				switch (keyEvent.Keycode)
				{
					case Key.Key1:
						TransitionToAIAssembly();
						break;
					case Key.Key2:
						TransitionToModuleViewer();
						break;
					case Key.Key3:
						_currentState = GameState.MainMenu;
						ShowMainMenu();
						break;
				}
			}
		}
	}
	
	private void HandleMenuSelection(int index)
	{
		if (index < 0 || index >= _mainMenuOptions.Count) return;
		
		var option = _mainMenuOptions[index];
		_currentState = option.TargetState;
		
		switch (option.TargetState)
		{
			case GameState.PreBattle:
				ShowPreBattle();
				break;
			case GameState.ModuleLab:
				ShowModuleLabMenu();
				break;
			case GameState.Archives:
				ShowComingSoon("ARCHIVES");
				break;
			case GameState.System:
				ShowComingSoon("SYSTEM SETTINGS");
				break;
		}
	}
	
	private void ShowModuleLabMenu()
	{
		_terminal.Clear();
		_terminal.PrintLine("=== MODULE LAB ===", Terminal.MessageType.System);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("[1] AI ASSEMBLY - Combine modules into AI", Terminal.MessageType.Normal);
		_terminal.PrintLine("[2] MODULE DATABASE - Browse all modules", Terminal.MessageType.Normal);
		_terminal.PrintLine("[3] BACK TO MAIN MENU", Terminal.MessageType.Normal);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("Select option (1-3)", Terminal.MessageType.Success);
		
		_currentState = GameState.ModuleLabMenu;
	}
	
	private void ShowComingSoon(string feature)
	{
		_terminal.Clear();
		_terminal.PrintLine($"=== {feature} ===", Terminal.MessageType.System);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("This feature is under development.", Terminal.MessageType.Warning);
		_terminal.PrintLine("Check back in a future update.", Terminal.MessageType.Normal);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("Tap to return to MAIN TERMINAL", Terminal.MessageType.System);
	}
	
	private void OnViewportSizeChanged()
	{
		UpdateInputAreaSize();
	}
	
	private void UpdateInputAreaSize()
	{
		if (ResolutionManager.Instance != null)
		{
			// Input area should be about 12% of screen height
			float inputAreaHeight = ResolutionManager.GetPercentageOfScreenHeight(12f);
			// Minimum 80 pixels for touch accessibility
			inputAreaHeight = Mathf.Max(80f, inputAreaHeight);
			
			// Update the input area size - InputArea is anchored to bottom
			// So we adjust the top offset to control height
			OffsetTop = -inputAreaHeight;
			
			GD.Print($"TerminalInput: Input area height set to {inputAreaHeight}px");
		}
	}
	
	private void OnTapDetected()
	{
		switch (_currentState)
		{
			case GameState.Startup:
				_currentState = GameState.MainMenu;
				ShowMainMenu();
				break;
				
			case GameState.MainMenu:
				// Don't auto-advance to battle on tap in main menu
				// Wait for specific menu selection
				break;
				
			case GameState.PreBattle:
				_currentState = GameState.Battle;
				StartBattle();
				break;
				
			case GameState.Battle:
				_currentState = GameState.PostBattle;
				ShowBattleResults();
				break;
				
			case GameState.PostBattle:
				_currentState = GameState.MainMenu;
				ShowMainMenu();
				break;
				
			case GameState.ModuleLab:
			case GameState.ModuleLabMenu:
			case GameState.Archives:
			case GameState.System:
				// Return to main menu from these states
				_currentState = GameState.MainMenu;
				ShowMainMenu();
				break;
		}
	}
	
	public void TransitionToMainMenu()
	{
		_currentState = GameState.MainMenu;
		ShowMainMenu();
	}
	
	private void ShowMainMenu()
	{
		_terminal.Clear();
		_terminal.PrintLine("=== MAIN TERMINAL ===", Terminal.MessageType.System);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		
		foreach (var option in _mainMenuOptions)
		{
			_terminal.PrintLine($"[{option.Key}] {option.Label}", Terminal.MessageType.Normal);
		}
		
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("Select option (1-4) or tap to return", Terminal.MessageType.Success);
	}
	
	private void ShowPreBattle()
	{
		_terminal.Clear();
		_terminal.PrintLine("=== PRE-BATTLE CONFIGURATION ===", Terminal.MessageType.System);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("Current AI Configuration:", Terminal.MessageType.Normal);
		_terminal.PrintLine("  Core: ALPHA_CORE v2.1", Terminal.MessageType.Success);
		_terminal.PrintLine("  Behavior: AGGRESSIVE_STANCE", Terminal.MessageType.Success);
		_terminal.PrintLine("  Augment: BUFFER_OVERFLOW", Terminal.MessageType.Success);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("Enemy Detected:", Terminal.MessageType.Warning);
		_terminal.PrintLine("  Type: WILD_AI_FRAGMENT", Terminal.MessageType.Error);
		_terminal.PrintLine("  Threat Level: MODERATE", Terminal.MessageType.Warning);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("Tap to ENGAGE", Terminal.MessageType.System);
	}
	
	private void StartBattle()
	{
		_terminal.ShowBattleIntro("ALPHA_BUILD", "CORRUPTED_DAEMON");
		
		CallDeferred(nameof(SimulateBattle));
	}
	
	private void SimulateBattle()
	{
		var timer = GetTree().CreateTimer(2.0f);
		timer.Timeout += () =>
		{
			_terminal.PrintLine("ROUND 1:", Terminal.MessageType.System);
			_terminal.PrintLine("  > Your AI executes BUFFER_OVERFLOW", Terminal.MessageType.Success);
			_terminal.PrintLine("  > Enemy takes 15 damage", Terminal.MessageType.Normal);
			_terminal.PrintLine("  > Enemy executes MALWARE_INJECTION", Terminal.MessageType.Error);
			_terminal.PrintLine("  > You take 12 damage", Terminal.MessageType.Warning);
			_terminal.PrintLine("", Terminal.MessageType.Normal);
			
			var timer2 = GetTree().CreateTimer(2.0f);
			timer2.Timeout += () =>
			{
				_terminal.PrintLine("ROUND 2:", Terminal.MessageType.System);
				_terminal.PrintLine("  > Your AI executes AGGRESSIVE_STRIKE", Terminal.MessageType.Success);
				_terminal.PrintLine("  > CRITICAL HIT! Enemy takes 25 damage", Terminal.MessageType.Success);
				_terminal.PrintLine("  > Enemy system corrupted", Terminal.MessageType.Error);
				_terminal.PrintLine("", Terminal.MessageType.Normal);
				_terminal.PrintLine("=== VICTORY ===", Terminal.MessageType.Success);
				_terminal.PrintLine("", Terminal.MessageType.Normal);
				_terminal.PrintLine("Tap to continue...", Terminal.MessageType.System);
			};
		};
	}
	
	private void ShowBattleResults()
	{
		_terminal.Clear();
		_terminal.PrintLine("=== BATTLE COMPLETE ===", Terminal.MessageType.Success);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("SALVAGE AVAILABLE:", Terminal.MessageType.System);
		_terminal.PrintLine("  > MALWARE_INJECTION (Augment)", Terminal.MessageType.Warning);
		_terminal.PrintLine("  > 15 BITS earned", Terminal.MessageType.Success);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("AI PERFORMANCE:", Terminal.MessageType.System);
		_terminal.PrintLine("  Damage Dealt: 40", Terminal.MessageType.Normal);
		_terminal.PrintLine("  Damage Taken: 12", Terminal.MessageType.Normal);
		_terminal.PrintLine("  Efficiency: 77%", Terminal.MessageType.Success);
		_terminal.PrintLine("", Terminal.MessageType.Normal);
		_terminal.PrintLine("Tap to return to MAIN TERMINAL", Terminal.MessageType.System);
	}
	
	private void TransitionToAIAssembly()
	{
		_terminal.Clear();
		_terminal.PrintLine("=== ACCESSING AI ASSEMBLY ===", Terminal.MessageType.System);
		_terminal.PrintLine("Loading assembly interface...", Terminal.MessageType.Normal);
		
		var timer = GetTree().CreateTimer(1.0f);
		timer.Timeout += () =>
		{
			GetTree().ChangeSceneToFile("res://Scenes/AIAssembly.tscn");
		};
	}
	
	private void TransitionToModuleViewer()
	{
		_terminal.Clear();
		_terminal.PrintLine("=== ACCESSING MODULE DATABASE ===", Terminal.MessageType.System);
		_terminal.PrintLine("Loading module viewer...", Terminal.MessageType.Normal);
		
		var timer = GetTree().CreateTimer(1.0f);
		timer.Timeout += () =>
		{
			GetTree().ChangeSceneToFile("res://Scenes/ModuleViewer.tscn");
		};
	}
	
	private enum GameState
	{
		Startup,
		MainMenu,
		PreBattle,
		Battle,
		PostBattle,
		ModuleLab,
		ModuleLabMenu,
		Archives,
		System
	}
	
	private class MenuOption
	{
		public string Key { get; set; }
		public string Label { get; set; }
		public GameState TargetState { get; set; }
		
		public MenuOption(string key, string label, GameState targetState)
		{
			Key = key;
			Label = label;
			TargetState = targetState;
		}
	}
}
