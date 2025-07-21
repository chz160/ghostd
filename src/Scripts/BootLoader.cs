using Godot;

public partial class BootLoader : Control
{
	private RichTextLabel _bootText;
	private AnimationPlayer _animationPlayer;
	
	public override void _Ready()
	{
		GD.Print("BootLoader: _Ready() called");
		
		_bootText = GetNode<RichTextLabel>("BootText");
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		// Set responsive font size for boot text
		UpdateBootTextSize();
		
		// Connect to viewport size changes
		GetViewport().SizeChanged += OnViewportSizeChanged;
		
		// Animation is handled by AnimationPlayer (autoplay)
		GD.Print("BootLoader: AnimationPlayer will handle boot sequence");
	}
	
	private void OnViewportSizeChanged()
	{
		UpdateBootTextSize();
	}
	
	// Called by AnimationPlayer at appropriate time
	public void OnMainTitleAnimationCompleted()
	{
		GD.Print("BootLoader: MainTitle animation completed");
		// This is now just a marker for logging, actual wait is in animation
	}
	
	// Called by AnimationPlayer at 8.0s
	public void ChangeScene()
	{
		GD.Print("BootLoader: ChangeScene() called");
		var result = GetTree().ChangeSceneToFile("res://Scenes/MainTerminal.tscn");
		
		if (result != Error.Ok)
		{
			GD.PrintErr($"BootLoader: Scene change failed with error: {result}");
		}
		else
		{
			GD.Print("BootLoader: Scene change initiated successfully");
		}
	}
	
	private void UpdateBootTextSize()
	{
		if (ResolutionManager.Instance != null && _bootText != null)
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
