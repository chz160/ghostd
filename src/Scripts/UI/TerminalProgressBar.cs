using Godot;
using System;

namespace Ghostd.UI
{
	/// <summary>
	/// Reusable ASCII-style progress bar for terminal interfaces
	/// </summary>
	public partial class TerminalProgressBar : Node
	{
		// Progress bar characters
		private const string FILLED_CHAR = "█";
		private const string EMPTY_CHAR = "░";
		private const string START_CHAR = "[";
		private const string END_CHAR = "]";
		
		// Default settings
		private const int DEFAULT_WIDTH = 20;
		private const float DEFAULT_DURATION = 2.0f;
		
		// Progress tracking
		private float _progress = 0.0f;
		private float _duration;
		private int _width;
		private bool _isRunning = false;
		private float _elapsedTime = 0.0f;
		
		// Callbacks
		public Action<float> OnProgressUpdate;
		public Action OnComplete;
		
		// Display components
		private Terminal _terminal;
		private string _label;
		private Terminal.MessageType _messageType;
		
		public TerminalProgressBar()
		{
			ProcessMode = ProcessModeEnum.Always;
		}
		
		/// <summary>
		/// Start a progress bar animation
		/// </summary>
		public void StartProgress(Terminal terminal, string label = "LOADING", float duration = DEFAULT_DURATION, int width = DEFAULT_WIDTH, Terminal.MessageType messageType = Terminal.MessageType.Success)
		{
			_terminal = terminal;
			_label = label;
			_duration = duration;
			_width = width;
			_messageType = messageType;
			_progress = 0.0f;
			_elapsedTime = 0.0f;
			_isRunning = true;
			
			// Draw initial state
			UpdateDisplay();
		}
		
		/// <summary>
		/// Stop the progress bar
		/// </summary>
		public void Stop()
		{
			_isRunning = false;
			_progress = 1.0f;
			UpdateDisplay();
		}
		
		/// <summary>
		/// Set progress manually (0.0 to 1.0)
		/// </summary>
		public void SetProgress(float progress)
		{
			_progress = Mathf.Clamp(progress, 0.0f, 1.0f);
			UpdateDisplay();
		}
		
		public override void _Process(double delta)
		{
			if (!_isRunning || _terminal == null) return;
			
			_elapsedTime += (float)delta;
			_progress = Mathf.Min(_elapsedTime / _duration, 1.0f);
			
			UpdateDisplay();
			OnProgressUpdate?.Invoke(_progress);
			
			if (_progress >= 1.0f)
			{
				_isRunning = false;
				OnComplete?.Invoke();
			}
		}
		
		private void UpdateDisplay()
		{
			if (_terminal == null) return;
			
			// Calculate filled/empty blocks
			int filledCount = Mathf.RoundToInt(_progress * _width);
			int emptyCount = _width - filledCount;
			
			// Build progress bar string
			string progressBar = START_CHAR;
			for (int i = 0; i < filledCount; i++)
				progressBar += FILLED_CHAR;
			for (int i = 0; i < emptyCount; i++)
				progressBar += EMPTY_CHAR;
			progressBar += END_CHAR;
			
			// Add percentage
			int percentage = Mathf.RoundToInt(_progress * 100);
			string displayText = $"{_label} {progressBar} {percentage}%";
			
			// Update terminal display
			_terminal.UpdateLastLine(displayText, _messageType);
		}
		
		/// <summary>
		/// Create a simple loading animation with callback
		/// </summary>
		public static TerminalProgressBar CreateAndStart(Node parent, Terminal terminal, string label = "LOADING", float duration = DEFAULT_DURATION, Action onComplete = null)
		{
			var progressBar = new TerminalProgressBar();
			parent.AddChild(progressBar);
			
			progressBar.OnComplete = () =>
			{
				onComplete?.Invoke();
				progressBar.QueueFree();
			};
			
			progressBar.StartProgress(terminal, label, duration);
			return progressBar;
		}
	}
}