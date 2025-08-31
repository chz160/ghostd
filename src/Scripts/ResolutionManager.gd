extends Node

## Simplified ResolutionManager that works with Godot's built-in scaling
## Based on research: Godot's viewport stretch mode handles DPI automatically

# Singleton instance
static var _instance: ResolutionManager = null

# Reference design resolution (iPhone 14/15 Pro size)
const REFERENCE_WIDTH = 390.0
const REFERENCE_HEIGHT = 844.0

# User-configurable safe area margins (as percentage of screen height)
@export var safe_area_top_percent: float = 3.0    # Default 3% for notch
@export var safe_area_bottom_percent: float = 2.0  # Default 2% for nav

# Simple scaling factor based on viewport size
static var ui_scale: float = 1.0

# Font scale is now same as UI scale (Godot handles DPI)
static var font_scale: float:
	get:
		return ui_scale

# Pixel scale for pixel art (rounded to nearest 0.5)
static var pixel_scale: float = 1.0

# Screen info
static var screen_size: Vector2

# Safe areas (now user-configurable via export properties)
static var safe_area_top: float = 0.0
static var safe_area_bottom: float = 0.0

# Singleton instance getter
static func get_instance() -> ResolutionManager:
	return _instance

func _enter_tree():
	if _instance != null and _instance != self:
		queue_free()
		return
	_instance = self

func _ready():
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	# Initial calculation
	update_resolution_scaling()
	
	# Listen for viewport size changes
	get_viewport().size_changed.connect(update_resolution_scaling)
	
	print("ResolutionManager (Simplified): Screen %s, UIScale: %.2f" % [screen_size, ui_scale])

func update_resolution_scaling():
	# Get current screen size
	var viewport = get_viewport()
	screen_size = viewport.get_visible_rect().size
	
	# Calculate simple UI scale based on smallest dimension
	# This ensures content fits regardless of orientation
	var width_scale = screen_size.x / REFERENCE_WIDTH
	var height_scale = screen_size.y / REFERENCE_HEIGHT
	ui_scale = min(width_scale, height_scale)
	
	# Pixel scale for pixel art - round to nearest 0.5 for clarity
	pixel_scale = round(ui_scale * 2.0) / 2.0
	pixel_scale = max(0.5, pixel_scale)
	
	# Calculate safe areas based on user-configured percentages
	safe_area_top = screen_size.y * (safe_area_top_percent / 100.0)
	safe_area_bottom = screen_size.y * (safe_area_bottom_percent / 100.0)
	
	print("ResolutionManager: UIScale=%.2f, PixelScale=%.2f, SafeAreas(T:%.0f, B:%.0f)" % 
		  [ui_scale, pixel_scale, safe_area_top, safe_area_bottom])

# Helper methods - kept for compatibility but simplified

static func get_scaled_font_size(base_font_size: int) -> int:
	# Simple scaling - let Godot handle DPI
	var scaled = roundi(base_font_size * ui_scale)
	return max(12, scaled) # Minimum 12px for readability

static func get_scaled_margin(base_margin: float) -> float:
	return base_margin * ui_scale

static func get_percentage_of_screen_width(percentage: float) -> float:
	return screen_size.x * (percentage / 100.0)

static func get_percentage_of_screen_height(percentage: float) -> float:
	return screen_size.y * (percentage / 100.0)

func _exit_tree():
	if _instance == self:
		_instance = null
