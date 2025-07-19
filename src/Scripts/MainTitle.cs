using Godot;

public partial class MainTitle : Control
{
	[Signal]
	public delegate void AnimationCompletedEventHandler();
	
	// CRT effect parameters
	private float scanlineIntensity = 0.3f;
	private float glowIntensity = 0.8f;
	private Color crtGreen = new Color(0.2f, 1.0f, 0.2f, 1.0f);
	private Color crtDarkGreen = new Color(0.1f, 0.4f, 0.1f, 1.0f);
	private float basePixelSize = 4.0f; // Base size, will be scaled
	private float flickerAmount = 0.05f;
	
	// Animation parameters
	private float drawProgress = 0.0f;
	private bool isAnimating = true;
	private float animationSpeed = 2.0f;
	
	// GHOSTD logo pixel data - clean main text without built-in glitch pixels
	private int[,] ghostPixels = new int[,] {
		// Main GHOSTD text area - compact and clean (7 rows x 40 columns to fit better)
		{0,1,1,1,1,0,0,1,1,0,1,1,0,0,1,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,0,0,1,1,1,1,0,0,0},
		{1,1,0,0,0,0,0,1,1,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0},
		{1,1,0,0,0,0,0,1,1,0,1,1,0,1,1,0,0,1,1,0,1,1,1,1,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0},
		{1,1,0,1,1,0,0,1,1,1,1,1,0,1,1,0,0,1,1,0,0,1,1,1,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0},
		{1,1,0,0,1,1,0,1,1,0,1,1,0,1,1,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0},
		{1,1,0,0,1,1,0,1,1,0,1,1,0,1,1,0,0,1,1,0,1,1,0,1,1,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0},
		{0,1,1,1,1,0,0,1,1,0,1,1,0,0,1,1,1,1,0,0,1,1,1,1,0,0,0,1,1,0,0,0,0,1,1,1,1,0,0,0}
	};
	
	// Scattered pixels for glitch effect - positioned around the main logo area
	private Vector2[] glitchPixels = {
		// Above the logo
		new Vector2(3, -2), new Vector2(12, -3), new Vector2(25, -1), new Vector2(35, -2), new Vector2(8, -1),
		new Vector2(18, -3), new Vector2(30, -2), new Vector2(40, -1), new Vector2(15, -1), new Vector2(22, -2),
		
		// To the sides of the logo
		new Vector2(-2, 2), new Vector2(-1, 4), new Vector2(42, 1), new Vector2(43, 3), new Vector2(44, 5),
		new Vector2(-3, 1), new Vector2(45, 2), new Vector2(-1, 6), new Vector2(42, 6), new Vector2(-2, 3),
		
		// Below the logo  
		new Vector2(5, 8), new Vector2(15, 9), new Vector2(28, 8), new Vector2(35, 10), new Vector2(10, 9),
		new Vector2(20, 10), new Vector2(32, 9), new Vector2(38, 8), new Vector2(12, 10), new Vector2(25, 9),
		
		// Scattered throughout
		new Vector2(7, 1), new Vector2(33, 4), new Vector2(16, 7), new Vector2(39, 2), new Vector2(2, 5)
	};
	
	public override void _Ready()
	{
		// Start the drawing animation
		var tween = CreateTween();
		tween.TweenProperty(this, "drawProgress", 1.0f, 3.0f);
		tween.TweenCallback(Callable.From(() => {
			isAnimating = false;
			EmitSignal(SignalName.AnimationCompleted);
		}));
	}
	
	public override void _Draw()
	{
		var viewport = GetViewportRect();
		var centerX = viewport.Size.X / 2;
		var centerY = viewport.Size.Y / 2;
		
		// Calculate responsive pixel size
		float pixelScale = ResolutionManager.Instance != null ? ResolutionManager.PixelScale : 1.0f;
		float pixelSize = basePixelSize * pixelScale;
		
		// Ensure logo fits on screen with margins
		int logoWidth = ghostPixels.GetLength(1);
		int logoHeight = ghostPixels.GetLength(0);
		
		// Check if logo would be too large and scale down if needed
		float maxLogoWidth = viewport.Size.X * 0.85f; // 85% of screen width
		float actualLogoWidth = logoWidth * pixelSize;
		if (actualLogoWidth > maxLogoWidth)
		{
			pixelSize = maxLogoWidth / logoWidth;
		}
		
		// Starting position to center the logo
		float startX = centerX - (logoWidth * pixelSize) / 2;
		float startY = centerY - (logoHeight * pixelSize) / 2;
		
		// Add slight flicker effect
		float flicker = 1.0f + (float)(GD.Randf() - 0.5) * flickerAmount;
		
		// Draw the main logo pixels
		int pixelsToShow = (int)(logoWidth * logoHeight * drawProgress);
		int currentPixel = 0;
		
		for (int y = 0; y < logoHeight; y++)
		{
			// Draw scanlines (every other line is darker)
			// Scale scanline size with pixel size
			if (y % 2 == 1)
			{
				var scanlineRect = new Rect2(0, startY + y * pixelSize, viewport.Size.X, pixelSize);
				DrawRect(scanlineRect, new Color(0, 0, 0, scanlineIntensity));
			}
			
			for (int x = 0; x < logoWidth; x++)
			{
				if (currentPixel >= pixelsToShow && isAnimating)
					break;
					
				if (ghostPixels[y, x] == 1)
				{
					var pixelRect = new Rect2(
						startX + x * pixelSize,
						startY + y * pixelSize,
						pixelSize,
						pixelSize
					);
					
					// Main pixel with glow effect
					var mainColor = crtGreen * flicker;
					DrawRect(pixelRect, mainColor);
					
					// Glow effect - draw slightly larger, transparent versions
					if (glowIntensity > 0)
					{
						var glowRect = new Rect2(
							pixelRect.Position - Vector2.One,
							pixelRect.Size + Vector2.One * 2
						);
						var glowColor = new Color(crtGreen.R, crtGreen.G, crtGreen.B, glowIntensity * 0.3f);
						DrawRect(glowRect, glowColor);
					}
				}
				currentPixel++;
			}
			
			if (currentPixel >= pixelsToShow && isAnimating)
				break;
		}
		
		// Draw scattered glitch pixels
		foreach (var glitchPos in glitchPixels)
		{
			if (GD.Randf() > 0.7f) // Randomly show/hide glitch pixels
			{
				var glitchRect = new Rect2(
					startX + glitchPos.X * pixelSize,
					startY + glitchPos.Y * pixelSize,
					pixelSize,
					pixelSize
				);
				
				var glitchColor = crtDarkGreen * flicker * (float)GD.RandRange(0.3, 0.8);
				DrawRect(glitchRect, glitchColor);
			}
		}
		
		// Draw overall CRT screen effect
		DrawCRTOverlay(viewport);
	}
	
	private void DrawCRTOverlay(Rect2 viewport)
	{
		// Subtle screen curvature effect with gradient
		var gradient = new Gradient();
		gradient.AddPoint(0.0f, new Color(0, 0, 0, 0.1f));
		gradient.AddPoint(0.3f, new Color(0, 0, 0, 0.0f));
		gradient.AddPoint(0.7f, new Color(0, 0, 0, 0.0f));
		gradient.AddPoint(1.0f, new Color(0, 0, 0, 0.1f));
		
		// Vignette effect
		for (int i = 0; i < 20; i++)
		{
			float alpha = (float)i / 20.0f * 0.02f;
			var borderColor = new Color(0, 0, 0, alpha);
			var borderRect = new Rect2(i, i, viewport.Size.X - i * 2, viewport.Size.Y - i * 2);
			DrawRect(borderRect, Colors.Transparent, false, 1.0f);
		}
	}
	
	public override void _Process(double delta)
	{
		// Continuously redraw for flicker effect
		QueueRedraw();
	}
	
	// Public methods to control the effect
	public void SetScanlineIntensity(float intensity)
	{
		scanlineIntensity = Mathf.Clamp(intensity, 0.0f, 1.0f);
	}
	
	public void SetGlowIntensity(float intensity)
	{
		glowIntensity = Mathf.Clamp(intensity, 0.0f, 2.0f);
	}
	
	public void SetPixelSize(float size)
	{
		basePixelSize = Mathf.Max(size, 1.0f);
	}
	
	public void RestartAnimation()
	{
		drawProgress = 0.0f;
		isAnimating = true;
		var tween = CreateTween();
		tween.TweenProperty(this, "drawProgress", 1.0f, 3.0f);
		tween.TweenCallback(Callable.From(() => isAnimating = false));
	}
}
