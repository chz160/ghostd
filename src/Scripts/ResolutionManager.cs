using Godot;

/// <summary>
/// Simplified ResolutionManager that works with Godot's built-in scaling
/// Based on research: Godot's viewport stretch mode handles DPI automatically
/// </summary>
public partial class ResolutionManager : Node
{
	private static ResolutionManager _instance;
	
	// Reference design resolution (iPhone 14/15 Pro size)
	private const float ReferenceWidth = 390f;
	private const float ReferenceHeight = 844f;
	
	// User-configurable safe area margins (as percentage of screen height)
	[Export] public float SafeAreaTopPercent = 3.0f;    // Default 3% for notch
	[Export] public float SafeAreaBottomPercent = 2.0f; // Default 2% for nav
	
	// Simple scaling factor based on viewport size
	public static float UIScale { get; private set; } = 1.0f;
	
	// Font scale is now same as UI scale (Godot handles DPI)
	public static float FontScale => UIScale;
	
	// Pixel scale for pixel art (rounded to nearest 0.5)
	public static float PixelScale { get; private set; } = 1.0f;
	
	// Screen info
	public static Vector2 ScreenSize { get; private set; }
	
	// Safe areas (now user-configurable via export properties)
	public static float SafeAreaTop { get; private set; } = 0f;
	public static float SafeAreaBottom { get; private set; } = 0f;
	
	// Singleton instance
	public static ResolutionManager Instance => _instance;
	
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
		ProcessMode = ProcessModeEnum.Always;
		
		// Initial calculation
		UpdateResolutionScaling();
		
		// Listen for viewport size changes
		GetViewport().SizeChanged += UpdateResolutionScaling;
		
		GD.Print($"ResolutionManager (Simplified): Screen {ScreenSize}, UIScale: {UIScale:F2}");
	}
	
	private void UpdateResolutionScaling()
	{
		// Get current screen size
		var viewport = GetViewport();
		ScreenSize = viewport.GetVisibleRect().Size;
		
		// Calculate simple UI scale based on smallest dimension
		// This ensures content fits regardless of orientation
		float widthScale = ScreenSize.X / ReferenceWidth;
		float heightScale = ScreenSize.Y / ReferenceHeight;
		UIScale = Mathf.Min(widthScale, heightScale);
		
		// Pixel scale for pixel art - round to nearest 0.5 for clarity
		PixelScale = Mathf.Round(UIScale * 2f) / 2f;
		PixelScale = Mathf.Max(0.5f, PixelScale);
		
		// Calculate safe areas based on user-configured percentages
		SafeAreaTop = ScreenSize.Y * (SafeAreaTopPercent / 100f);
		SafeAreaBottom = ScreenSize.Y * (SafeAreaBottomPercent / 100f);
		
		GD.Print($"ResolutionManager: UIScale={UIScale:F2}, PixelScale={PixelScale:F2}, " +
				 $"SafeAreas(T:{SafeAreaTop:F0}, B:{SafeAreaBottom:F0})");
	}
	
	// Helper methods - kept for compatibility but simplified
	
	public static int GetScaledFontSize(int baseFontSize)
	{
		// Simple scaling - let Godot handle DPI
		int scaled = Mathf.RoundToInt(baseFontSize * UIScale);
		return Mathf.Max(12, scaled); // Minimum 12px for readability
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
	
	public override void _ExitTree()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
}
