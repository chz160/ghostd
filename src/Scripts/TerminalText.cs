using Godot;
using System;
using System.Collections.Generic;

public partial class TerminalText : CanvasLayer
{
	const double CharacterReadRate = 0.025;
	
	[Signal]
	public delegate void TerminalTextFinishedEventHandler();

	private enum TerminalState
	{
		Ready,
		Reading,
		Finished
	}

	private MarginContainer _textboxContainer;
	private Label _label;
	private Tween _tween;
	private TerminalState _currentState = TerminalState.Ready;
	private Queue<string> _queuedTexts = new Queue<string>();

	public override void _Ready()
	{
		GD.Print($"Starting state: TerminalState.{_currentState}");
		InitializeTween();
		_textboxContainer = GetNode<MarginContainer>("TextboxContainer");
		_label = GetNode<Label>("TextboxContainer/MarginContainer/HBoxContainer/Label");
		HideTextBox();
		QueueText("       **** GHOSTD V1 ****\nBOOTING...\nINITIALIZING 128G RAM...\n    - 124G FREE\nLOADING TERMINAL...\n\nREADY.");
	}

	public override void _Process(double delta)
	{
		switch (_currentState)
		{
			case TerminalState.Ready:
				if (_queuedTexts.Count > 0)
				{
					DisplayText();
				}
				break;
			case TerminalState.Reading:
				// Waiting for user input to finish reading
				break;
			case TerminalState.Finished:
				break;
			default:
				GD.PrintErr($"TestBoot: Unknown state {_currentState}");
				break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventScreenTouch screenEvent && screenEvent.IsPressed() ||
			@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.IsPressed())
		{
			if (_currentState == TerminalState.Reading)
			{
				GD.Print("TestBoot: User pressed a screen, finishing text reading");
				_label.VisibleRatio = 1; // Show all text immediately
				_tween.Stop();
				OnTweenFinished(_tween);
			}
			else if (_currentState == TerminalState.Finished)
			{
				if (_queuedTexts.Count > 0)
				{
					GD.Print("TestBoot: Tween finished, displaying next text");
					InitializeTween();
					DisplayText();
				}
			}
		}
	}

	public void QueueText(string nextText)
	{
		_queuedTexts.Enqueue(nextText);
	}

	private void HideTextBox()
	{
		_label.Text = "";
		_textboxContainer.Hide();
	}

	private void ShowTextBox()
	{
		_textboxContainer.Show();
	}

	private void DisplayText()
	{
		var nextText = _queuedTexts.Dequeue();
		_label.Text = nextText;
		ChangeState(TerminalState.Reading);
		ShowTextBox();
		_label.VisibleRatio = 0;
		_tween.TweenProperty(
			_label,
			"visible_ratio",
			1,
			nextText.Length * CharacterReadRate);
	}

	private void OnTweenFinished(Tween tween)
	{
		ChangeState(TerminalState.Finished);
		GD.Print($"Ending state: TerminalState.{_currentState}");

		if (_queuedTexts.Count == 0)
		{
			//Signal that the terminal session is complete
			EmitSignal(SignalName.TerminalTextFinished);
		}
	}

	private void ChangeState(TerminalState nextState)
	{
		_currentState = nextState;
		GD.Print($"Changing state to TerminalState.{nextState}");
		// switch (_currentState) {
		// 	case TerminalState.Ready:
		// 		GD.Print("Changing state to TerminalState");
		// 		break;
		// 	case TerminalState.Reading:
		// 		GD.Print("TestBoot: State changed to Reading");
		// 		break;
		// 	case TerminalState.Finished:
		// 		GD.Print("TestBoot: State changed to Finished");
		// 		break;
		// 	default:
		// 		GD.PrintErr($"TestBoot: Unknown state {nextState}");
		// 		break;
		// }
	}

	private void InitializeTween()
	{
		_tween = GetTree().CreateTween();
		_tween.Finished += () => { OnTweenFinished(_tween); };
	}
}
