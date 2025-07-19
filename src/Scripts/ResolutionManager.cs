using Godot;

public partial class ResolutionManager : Node
{
	private static ResolutionManager _instance;
	
	// Reference design resolution (iPhone 14/15 Pro size)
	private const float ReferenceWidth = 390f;
	private const float ReferenceHeight = 844f;
	
	// Scaling factors
	public static float UIScale { get; private set; } = 1.0f;
	public static float FontScale { get; private set; } = 1.0f;
	public static float PixelScale { get; private set; } = 1.0f;
	
	// Safe area margins (for notches, system UI, etc.)
	public static float SafeAreaTop { get; private set; } = 0f;
	public static float SafeAreaBottom { get; private set; } = 0f;
	public static float SafeAreaLeft { get; private set; } = 0f;
	public static float SafeAreaRight { get; private set; } = 0f;
	
	// Screen info
	public static Vector2 ScreenSize { get; private set; }
	public static float AspectRatio { get; private set; }
	public static float DPI { get; private set; }
	
	// Singleton instance
	public static ResolutionManager Instance 
	{ 
		get 
		{ 
			if (_instance == null)
			{
				GD.PrintErr("ResolutionManager: Instance is null! Make sure it's added to the scene.");
			}
			return _instance; 
		} 
	}
	
	public override void _EnterTree()
	{
		if (_instance != null && _instance != this)
		{
			QueueFree();
			return;
		}
		_instance = this;
	}
	
	public override void _Ready()
	{
		// Make this node persistent across scene changes
		ProcessMode = ProcessModeEnum.Always;
		
		// Calculate initial scaling factors
		UpdateResolutionScaling();
		
		// Connect to viewport size changed signal
		GetViewport().SizeChanged += OnViewportSizeChanged;
		
		GD.Print($"ResolutionManager: Initialized with screen size {ScreenSize}, UI Scale: {UIScale:F2}");
	}
	
	private void OnViewportSizeChanged()
	{
		UpdateResolutionScaling();
	}
	
	private void UpdateResolutionScaling()
	{
		var viewport = GetViewport();
		ScreenSize = viewport.GetVisibleRect().Size;
		AspectRatio = ScreenSize.X / ScreenSize.Y;
		
		// Get DPI (this is approximate, as Godot doesn't always report accurate DPI)
		DPI = DisplayServer.ScreenGetDpi();
		if (DPI <= 0) DPI = 160f; // Default Android MDPI
		
		// Calculate scaling factors
		float widthScale = ScreenSize.X / ReferenceWidth;
		float heightScale = ScreenSize.Y / ReferenceHeight;
		
		// Use the smaller scale to ensure content fits
		UIScale = Mathf.Min(widthScale, heightScale);
		
		// Font scale considers DPI for readability
		float dpiScale = DPI / 160f; // 160 is Android's MDPI baseline
		FontScale = UIScale * Mathf.Sqrt(dpiScale); // Square root to prevent fonts from scaling too aggressively
		
		// Pixel scale for pixel art (like the logo)
		// Round to nearest 0.5 to maintain pixel art clarity
		PixelScale = Mathf.Round(UIScale * 2f) / 2f;
		PixelScale = Mathf.Max(0.5f, PixelScale); // Minimum 0.5x scale
		
		// Calculate safe areas (approximate - would need platform-specific APIs for accurate values)
		CalculateSafeAreas();
		
		GD.Print($"ResolutionManager: Updated - Screen: {ScreenSize}, DPI: {DPI}, Scales - UI: {UIScale:F2}, Font: {FontScale:F2}, Pixel: {PixelScale:F2}");
	}
	
	private void CalculateSafeAreas()
	{
		// Basic safe area calculation
		// In a real implementation, you'd use platform-specific APIs
		
		// For phones with aspect ratio > 2:1, assume notch/punch hole
		if (AspectRatio < 0.5f) // Portrait mode, tall screen
		{
			// Assume 44pt safe area at top for notch (converted to pixels)
			SafeAreaTop = 44f * (DPI / 160f);
			// Assume 34pt at bottom for gesture navigation
			SafeAreaBottom = 34f * (DPI / 160f);
		}
		else
		{
			// Standard 16:9 or similar, minimal safe areas
			SafeAreaTop = 20f * UIScale;
			SafeAreaBottom = 20f * UIScale;
		}
		
		// Horizontal safe areas (usually none in portrait)
		SafeAreaLeft = 0f;
		SafeAreaRight = 0f;
	}
	
	// Helper methods for UI elements
	
	public static float GetScaledValue(float baseValue, ScaleType scaleType = ScaleType.UI)
	{
		return scaleType switch
		{
			ScaleType.UI => baseValue * UIScale,
			ScaleType.Font => baseValue * FontScale,
			ScaleType.Pixel => baseValue * PixelScale,
			_ => baseValue
		};
	}
	
	public static int GetScaledFontSize(int baseFontSize)
	{
		int scaled = Mathf.RoundToInt(baseFontSize * FontScale);
		// Ensure minimum readability
		return Mathf.Max(12, scaled);
	}
	
	public static float GetScaledMargin(float baseMargin)
	{
		return baseMargin * UIScale;
	}
	
	public static float GetPercentageOfScreenWidth(float percentage)
	{
		return ScreenSize.X * (percentage / 100f);
	}
	
	public static float GetPercentageOfScreenHeight(float percentage)
	{
		return ScreenSize.Y * (percentage / 100f);
	}
	
	public static Rect2 GetSafeRect()
	{
		return new Rect2(
			SafeAreaLeft,
			SafeAreaTop,
			ScreenSize.X - SafeAreaLeft - SafeAreaRight,
			ScreenSize.Y - SafeAreaTop - SafeAreaBottom
		);
	}
	
	public enum ScaleType
	{
		UI,
		Font,
		Pixel
	}
	
	public override void _ExitTree()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
}