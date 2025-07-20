using Godot;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public partial class Terminal : Control
{
	private RichTextLabel _output;
	private Queue<QueuedText> _commandQueue = new Queue<QueuedText>();
	private float _typewriterDelay = 0.03f;
	private float _typewriterTimer = 0f;
	private StringBuilder _displayedText = new StringBuilder();
	
	// Current line being typed
	private string _currentFormattedText = "";
	private string _currentPlainText = "";
	private int _charIndex = 0;
	
	// Progress bar flag
	private bool _startProgressBarWhenReady = false;
	
	// Colors for message types
	private readonly Color _systemColor = new Color(0.2f, 1f, 0.2f);
	private readonly Color _errorColor = new Color(1f, 0.2f, 0.2f);
	private readonly Color _warningColor = new Color(1f, 1f, 0.2f);
	
	public override void _Ready()
	{
		_output = GetNode<RichTextLabel>("TerminalOutput");
		_output.Clear();
		_output.BbcodeEnabled = true;
		
		// Set responsive font size
		UpdateFontSize();
		
		GD.Print("Terminal: _Ready() called - displaying startup sequence");
		
		// Display startup sequence
		ShowStartupSequence();
		
		// Connect to viewport size changes
		GetViewport().SizeChanged += OnViewportSizeChanged;
	}
	
	private void OnViewportSizeChanged()
	{
		UpdateFontSize();
	}
	
	private void UpdateFontSize()
	{
		if (ResolutionManager.Instance != null)
		{
			int baseFontSize = 16;
			int scaledFontSize = ResolutionManager.GetScaledFontSize(baseFontSize);
			
			// Update the font size
			_output.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
			_output.AddThemeFontSizeOverride("normal_font_size", scaledFontSize);
			_output.AddThemeFontSizeOverride("bold_font_size", scaledFontSize);
			_output.AddThemeFontSizeOverride("italics_font_size", scaledFontSize);
			_output.AddThemeFontSizeOverride("mono_font_size", scaledFontSize);
			
			// Update margins based on screen size
			float baseMargin = 20f;
			float scaledMargin = ResolutionManager.GetScaledMargin(baseMargin);
			
			// Apply responsive margins
			_output.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			_output.OffsetLeft = scaledMargin;
			_output.OffsetTop = scaledMargin + ResolutionManager.SafeAreaTop;
			_output.OffsetRight = -scaledMargin;
			_output.OffsetBottom = -scaledMargin;
			
			GD.Print($"Terminal: Font size updated to {scaledFontSize}px, margins to {scaledMargin}px");
		}
	}
	
	public void ShowStartupSequence()
	{
		// Clear any existing content
		Clear();
		
		// Display ASCII logo instantly
		//DisplayLogo();
		
		// Then show the rest with typewriter effect
		PrintLine("", MessageType.Normal);
		PrintLine("GHOSTD TERMINAL v1.0.0", MessageType.System);
		PrintLine("=====================================", MessageType.System);
		PrintLine("", MessageType.Normal);
		PrintLine("Initializing core systems...", MessageType.System);
		PrintLine("Loading AI modules...", MessageType.System);
		PrintLine("", MessageType.Normal);
		
		// Add a placeholder line for the progress bar
		PrintLine("INITIALIZING [                         ] 0%", MessageType.System);
		
		// Set a flag to start progress bar when typewriter completes
		_startProgressBarWhenReady = true;
	}
	
	private void StartProgressBar()
	{
		// Create and start the progress bar
		var progressBar = new Ghostd.UI.TerminalProgressBar();
		AddChild(progressBar);
		
		progressBar.OnComplete = () =>
		{
			// Auto-transition to main menu after progress completes
			GetTree().CreateTimer(0.5f).Timeout += () =>
			{
				// Find the InputArea node which has the TerminalInput script
				var terminalInput = GetNode<TerminalInput>("InputArea");
				if (terminalInput != null)
				{
					terminalInput.TransitionToMainMenu();
				}
			};
			progressBar.QueueFree();
		};
		
		// Use width of 18 for safety on small screens
		// Format: "INITIALIZING [██████████████████] 100%"
		progressBar.StartProgress(this, "INITIALIZING", 3.0f, 18);
	}
	
	public override void _Process(double delta)
	{
		// Handle typewriter effect
		if (!string.IsNullOrEmpty(_currentPlainText))
		{
			_typewriterTimer += (float)delta;
			
			// For very fast typewriter speeds, process multiple characters per frame
			int charsToProcess = 1;
			if (_typewriterDelay < 0.01f) // If delay is less than 10ms
			{
				// Calculate how many characters we should show this frame
				charsToProcess = Mathf.Max(1, (int)(delta / _typewriterDelay));
			}
			
			while (_typewriterTimer >= _typewriterDelay && _charIndex < _currentPlainText.Length && charsToProcess > 0)
			{
				_typewriterTimer -= _typewriterDelay;
				_charIndex++;
				charsToProcess--;
				
				// Check if line is complete
				if (_charIndex >= _currentPlainText.Length)
				{
					// Add the complete formatted line to our history
					_displayedText.Append(_currentFormattedText);
					
					// Reset for next line
					_currentFormattedText = "";
					_currentPlainText = "";
					_charIndex = 0;
					_typewriterTimer = 0f;
					
					// Process next command if available
					if (_commandQueue.Count > 0)
					{
						ProcessNextCommand();
					}
					break;
				}
			}
			
			// Update display after processing characters
			if (_charIndex > 0 && !string.IsNullOrEmpty(_currentPlainText))
			{
				UpdateDisplay();
			}
		}
		else if (_commandQueue.Count > 0)
		{
			ProcessNextCommand();
		}
		else if (_startProgressBarWhenReady)
		{
			// All typewriter animations complete, start progress bar
			_startProgressBarWhenReady = false;
			StartProgressBar();
		}
	}
	
	private void UpdateDisplay()
	{
		_output.Clear();
		_output.AppendText(_displayedText.ToString());
		
		// Add the current line with typewriter effect
		if (!string.IsNullOrEmpty(_currentPlainText))
		{
			// For colored text, we need to wrap the partial text in color tags
			if (_currentFormattedText.Contains("[color="))
			{
				// Extract the color tag
				var colorMatch = System.Text.RegularExpressions.Regex.Match(_currentFormattedText, @"\[color=(#[0-9a-fA-F]+)\]");
				if (colorMatch.Success)
				{
					string color = colorMatch.Groups[1].Value;
					string partialText = _currentPlainText.Substring(0, _charIndex);
					_output.AppendText($"[color={color}]{partialText}[/color]");
				}
				else
				{
					// Fallback to showing the whole line
					_output.AppendText(_currentFormattedText);
				}
			}
			else
			{
				// For non-colored text, show character by character
				_output.AppendText(_currentPlainText.Substring(0, _charIndex));
			}
		}
	}
	
	public void PrintLine(string text, MessageType type = MessageType.Normal)
	{
		string coloredText = FormatText(text, type);
		string plainText = StripBBCode(coloredText) + "\n";
		_commandQueue.Enqueue(new QueuedText { Formatted = coloredText + "\n", Plain = plainText });
	}
	
	public void PrintText(string text, MessageType type = MessageType.Normal)
	{
		string coloredText = FormatText(text, type);
		string plainText = StripBBCode(coloredText);
		_commandQueue.Enqueue(new QueuedText { Formatted = coloredText, Plain = plainText });
	}
	
	public void PrintInstant(string text, MessageType type = MessageType.Normal)
	{
		// Display text instantly without typewriter effect
		string coloredText = FormatText(text, type);
		_displayedText.Append(coloredText + "\n");
		_output.Clear();
		_output.AppendText(_displayedText.ToString());
	}
	
	/// <summary>
	/// Updates the last line in the terminal (useful for progress bars)
	/// </summary>
	public void UpdateLastLine(string text, MessageType type = MessageType.Normal)
	{
		// Cancel any ongoing typewriter animation
		_currentFormattedText = "";
		_currentPlainText = "";
		_charIndex = 0;
		_commandQueue.Clear();
		
		if (_displayedText.Length == 0)
		{
			// If no text exists, just print normally
			PrintInstant(text, type);
			return;
		}
		
		// Find the last newline in the displayed text
		string displayedStr = _displayedText.ToString();
		int searchStart = displayedStr.Length > 1 ? displayedStr.Length - 2 : 0;
		int lastNewlineIndex = searchStart > 0 ? displayedStr.LastIndexOf('\n', searchStart) : -1;
		
		if (lastNewlineIndex >= 0)
		{
			// Remove everything after the last newline
			_displayedText.Length = lastNewlineIndex + 1;
			
			// Add the new line
			string coloredText = FormatText(text, type);
			_displayedText.Append(coloredText + "\n");
			
			// Refresh display
			_output.Clear();
			_output.AppendText(_displayedText.ToString());
		}
		else
		{
			// If no lines exist, just print normally
			PrintInstant(text, type);
		}
	}
	
	public void Clear()
	{
		if (_output != null)
			_output.Clear();
		_displayedText.Clear();
		_commandQueue.Clear();
		_currentFormattedText = "";
		_currentPlainText = "";
		_charIndex = 0;
	}
	
	public void ShowBattleIntro(string aiName, string enemyName)
	{
		Clear();
		PrintLine("=== BATTLE INITIATED ===", MessageType.System);
		PrintLine("", MessageType.Normal);
		PrintLine($"Your AI: {aiName}", MessageType.System);
		PrintLine($"Enemy: {enemyName}", MessageType.Error);
		PrintLine("", MessageType.Normal);
		PrintLine("Analyzing combat parameters...", MessageType.Normal);
		PrintLine("Loading battle protocols...", MessageType.Normal);
		PrintLine("", MessageType.Normal);
	}
	
	private void ProcessNextCommand()
	{
		if (_commandQueue.Count > 0)
		{
			var next = _commandQueue.Dequeue();
			_currentFormattedText = next.Formatted;
			_currentPlainText = next.Plain;
			_charIndex = 0;
		}
	}
	
	private string StripBBCode(string text)
	{
		// Remove BBCode tags but keep the content
		return Regex.Replace(text, @"\[.*?\]", "");
	}
	
	private string FormatText(string text, MessageType type)
	{
		return type switch
		{
			MessageType.System => $"[color=#22ff22]{text}[/color]",
			MessageType.Error => $"[color=#ff2222]{text}[/color]",
			MessageType.Warning => $"[color=#ffff22]{text}[/color]",
			MessageType.Success => $"[color=#22ffff]{text}[/color]",
			_ => text
		};
	}
	
	//private void DisplayLogo()
	//{
		//// ASCII art logo
		//string[] logoLines = new string[]
		//{
			//"                                          ▒░",
			//"             ░▒░                    ▒▒░░                 ░░░     ░░░",
			//"            ░░   ░░░░                      ░░   ░░░░",
			//"        ░░░              ▒ ░░        ░░░             ░▒░░░   ▒▒▒░░▒░   ░   ░",
			//"     ░░░░░░░░░ ░ ░░░░  ░░░░░░  ░░░░░░░░ ░░░ ░░░░░░░░  ░░░░░░░░░░░ ░░░░░░░░░░",
			//"  ░░ ▒███████▒░  ▓██▓░  ▒██▓░ ░▒███████▒   ░▓██████▒░ ░█████████▓ ▒████████▓░ ░",
			//"    ▒███▓▓▓▓███▒░▒██▒░  ▒██▓░ ▓███▓▓▓████ ▒███▓▓▓████░░▓▓▓████▓▓▒ ░████▓▓████▒░",
			//" ░░ ▒██▓    ███░ ▓██▓░  ▒██▓░ ▓██▒   ░███ ▒██▓░  ░██▓░    ▓██░    ░███░  ░▓██▒",
			//"    ▒██▓    ░░░░ ▓██▓░  ▒██▓░ ███░   ░███ ▒██▓░        ░░ ▓██▒  ░ ░███░   ▓██▒░░░",
			//"  ░▒▒██▓  ░░     ▓███▓▓▓███▒░ ▓██▒ ░░░███ ▒████▓▒░  ░░    ▓██▒    ░███░   ▒██▒",
			//" ░░ ▒██▓ ░▓██▓▓░ ▓█████████▓░ ▓██▒ ░ ░███  ░▓██████▓░  ░░░▓██▒ ░░ ░███░   ▓██▒",
			//"    ▒██▓ ░████▓░ ▓██▓░  ▒██▓░ ▓██▒ ░ ░██▓     ░▒▓▓███░   ░▓██▒ ░░░░███░   ▓██▒ ░░",
			//"░░░░ ▒██▓ ░░░███░ ▓██▓░  ▒██▓░ ▓██▒   ░███   ░░   ░███░    ▓██░    ░███░   ▓██▒",
			//" ░░ ▓██▓    ███░ ▓██▒░  ▒██▓░ ▓██▒   ░███ ░█▒▒░  ░███░░▒░ ▓██▒ ░  ░███░ ░░▓██▒░░",
			//"    ▒██▓░░░░███░ ▓██▓░░░▒██▓░ ▓██▒░░░▒███ ▒██▓░░░▒███░    ▓██▒  ░░░███▒░░▒███▒",
			//"    ░▓█████████░ ▓██▓░░░░██▓░ ▒█████████░ ░▓████████▒░░░  ▓██▒  ░░░█████████▒░",
			//" ░░░ ░▒▓▓▓▓▓▓▒░  ▒▓▓▒░  ░▒▓▒░  ░▒▓▓▓▓▓▒░    ░▒▓▓▓▓▓░   ░░ ▒▓▓░░   ░▒▓▓▓▓▓▓▓░ ░▒░",
			//"               ░░    ░  ░░░░    ░░░  ░░░ ░░    ░░░░   ░ ░░░      ░░░     ░░░",
			//"       ░░                        ░             ░░░              ░░░",
			//"            ░             ░░      ░░                                    ▒",
			//"                                         ░░░░░░░              ░░"
		//};
		//
		//// Join all lines into a single string and display instantly
		//string fullLogo = string.Join("\n", logoLines);
		//PrintInstant(fullLogo, MessageType.Success);
	//}
	
	private class QueuedText
	{
		public string Formatted { get; set; }
		public string Plain { get; set; }
	}
	
	public enum MessageType
	{
		Normal,
		System,
		Error,
		Warning,
		Success
	}
}
