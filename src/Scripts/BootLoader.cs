using Godot;

public partial class BootLoader : Control
{
	private RichTextLabel _bootText;
	private float _bootTimer = 0f;
	private float _bootDuration = 2.0f;
	
	public override void _Ready()
	{
		GD.Print("BootLoader: _Ready() called");
		
		_bootText = GetNode<RichTextLabel>("BootText");
		
		// Set responsive font size for boot text
		UpdateBootTextSize();
		
		var timer = GetTree().CreateTimer(0.5f);
		timer.Timeout += () =>
		{
			GD.Print("BootLoader: First timer fired (0.5s)");
			_bootText.Text = "[center]INITIALIZING GHOSTD...[/center]";
		};
		
		var timer2 = GetTree().CreateTimer(1.5f);
		timer2.Timeout += () =>
		{
			GD.Print("BootLoader: Second timer fired (1.5s)");
			_bootText.Text = "[center]LOADING TERMINAL...[/center]";
		};
		
		var timer3 = GetTree().CreateTimer(_bootDuration);
		timer3.Timeout += () =>
		{
			GD.Print("BootLoader: Third timer fired (2.0s) - Showing MainTitle");
			ShowMainTitle();
		};
	}
	
	private void ShowMainTitle()
	{
		// Hide boot text
		_bootText.Visible = false;
		
		// Create and add MainTitle
		var mainTitle = new MainTitle();
		mainTitle.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		AddChild(mainTitle);
		
		// Connect to animation completed signal
		mainTitle.AnimationCompleted += OnMainTitleAnimationCompleted;
		
		GD.Print("BootLoader: MainTitle added and animation started");
	}
	
	private void OnMainTitleAnimationCompleted()
	{
		GD.Print("BootLoader: MainTitle animation completed, transitioning to MainTerminal");
		
		// Add a short delay before transitioning
		var timer = GetTree().CreateTimer(1.0f);
		timer.Timeout += () =>
		{
			ChangeScene();
		};
	}
	
	private void ChangeScene()
	{
		GD.Print("BootLoader: ChangeScene() called");
		var result = GetTree().ChangeSceneToFile("res://Scenes/MainTerminal.tscn");
		
		if (result != Error.Ok)
		{
			GD.PrintErr($"BootLoader: Scene change failed with error: {result}");
			// Can't update _bootText since it's hidden, maybe show an error dialog
		}
		else
		{
			GD.Print("BootLoader: Scene change initiated successfully");
		}
	}
	
	private void UpdateBootTextSize()
	{
		if (ResolutionManager.Instance != null)
		{
			int baseFontSize = 24;
			int scaledFontSize = ResolutionManager.GetScaledFontSize(baseFontSize);
			
			_bootText.AddThemeFontSizeOverride("normal_font_size", scaledFontSize);
			
			// Also update the text area size
			float boxWidth = ResolutionManager.GetPercentageOfScreenWidth(80f); // 80% of screen width
			float boxHeight = ResolutionManager.GetPercentageOfScreenHeight(15f); // 15% of screen height
			
			_bootText.OffsetLeft = -boxWidth / 2;
			_bootText.OffsetRight = boxWidth / 2;
			_bootText.OffsetTop = -boxHeight / 2;
			_bootText.OffsetBottom = boxHeight / 2;
			
			GD.Print($"BootLoader: Font size set to {scaledFontSize}px, box size {boxWidth}x{boxHeight}");
		}
	}
}
