extends Control

signal animation_completed

# CRT effect parameters
var scanline_intensity = 0.3
var glow_intensity = 0.8
var crt_green = Color(0.2, 1.0, 0.2, 1.0)
var crt_dark_green = Color(0.1, 0.4, 0.1, 1.0)
var base_pixel_size = 4.0 # Base size, will be scaled
var flicker_amount = 0.05

# Animation parameters
var _draw_progress = 0.0
var animation_speed = 2.0

# Make draw_progress animatable by AnimationPlayer
@export var draw_progress: float:
	get:
		return _draw_progress
	set(value):
		_draw_progress = clamp(value, 0.0, 1.0)
		queue_redraw()

# GHOSTD logo pixel data - clean main text without built-in glitch pixels
var ghost_pixels = [
	# Main GHOSTD text area - compact and clean (7 rows x 40 columns to fit better)
	[0,1,1,1,1,0,0,1,1,0,1,1,0,0,1,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1,0,0,1,1,1,1,0,0,0],
	[1,1,0,0,0,0,0,1,1,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0],
	[1,1,0,0,0,0,0,1,1,0,1,1,0,1,1,0,0,1,1,0,1,1,1,1,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0],
	[1,1,0,1,1,0,0,1,1,1,1,1,0,1,1,0,0,1,1,0,0,1,1,1,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0],
	[1,1,0,0,1,1,0,1,1,0,1,1,0,1,1,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0],
	[1,1,0,0,1,1,0,1,1,0,1,1,0,1,1,0,0,1,1,0,1,1,0,1,1,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0],
	[0,1,1,1,1,0,0,1,1,0,1,1,0,0,1,1,1,1,0,0,1,1,1,1,0,0,0,1,1,0,0,0,0,1,1,1,1,0,0,0]
]

# Scattered pixels for glitch effect - positioned around the main logo area
var glitch_pixels = [
	# Above the logo
	Vector2(3, -2), Vector2(12, -3), Vector2(25, -1), Vector2(35, -2), Vector2(8, -1),
	Vector2(18, -3), Vector2(30, -2), Vector2(40, -1), Vector2(15, -1), Vector2(22, -2),
	
	# To the sides of the logo
	Vector2(-2, 2), Vector2(-1, 4), Vector2(42, 1), Vector2(43, 3), Vector2(44, 5),
	Vector2(-3, 1), Vector2(45, 2), Vector2(-1, 6), Vector2(42, 6), Vector2(-2, 3),
	
	# Below the logo  
	Vector2(5, 8), Vector2(15, 9), Vector2(28, 8), Vector2(35, 10), Vector2(10, 9),
	Vector2(20, 10), Vector2(32, 9), Vector2(38, 8), Vector2(12, 10), Vector2(25, 9),
	
	# Scattered throughout
	Vector2(7, 1), Vector2(33, 4), Vector2(16, 7), Vector2(39, 2), Vector2(2, 5)
]

func _ready():
	# Animation is now controlled by AnimationPlayer, not started automatically
	print("MainTitle: Ready, waiting for AnimationPlayer to control draw_progress")

func _draw():
	var viewport = get_viewport_rect()
	var center_x = viewport.size.x / 2
	var center_y = viewport.size.y / 2
	
	# Calculate responsive pixel size
	var pixel_scale = ResolutionManager.pixel_scale if ResolutionManager.get_instance() != null else 1.0
	var pixel_size = base_pixel_size * pixel_scale
	
	# Ensure logo fits on screen with margins
	var logo_width = ghost_pixels[0].size()
	var logo_height = ghost_pixels.size()
	
	# Check if logo would be too large and scale down if needed
	var max_logo_width = viewport.size.x * 0.85 # 85% of screen width
	var actual_logo_width = logo_width * pixel_size
	if actual_logo_width > max_logo_width:
		pixel_size = max_logo_width / logo_width
	
	# Starting position to center the logo
	var start_x = center_x - (logo_width * pixel_size) / 2
	var start_y = center_y - (logo_height * pixel_size) / 2
	
	# Add slight flicker effect
	var flicker = 1.0 + (randf() - 0.5) * flicker_amount
	
	# Draw the main logo pixels
	var pixels_to_show = int(logo_width * logo_height * draw_progress)
	var current_pixel = 0
	
	for y in logo_height:
		# Draw scanlines (every other line is darker)
		# Scale scanline size with pixel size
		if y % 2 == 1:
			var scanline_rect = Rect2(0, start_y + y * pixel_size, viewport.size.x, pixel_size)
			draw_rect(scanline_rect, Color(0, 0, 0, scanline_intensity))
		
		for x in logo_width:
			if current_pixel >= pixels_to_show:
				break
			
			if ghost_pixels[y][x] == 1:
				var pixel_rect = Rect2(
					start_x + x * pixel_size,
					start_y + y * pixel_size,
					pixel_size,
					pixel_size
				)
				
				# Main pixel with glow effect
				var main_color = crt_green * flicker
				draw_rect(pixel_rect, main_color)
				
				# Glow effect - draw slightly larger, transparent versions
				if glow_intensity > 0:
					var glow_rect = Rect2(
						pixel_rect.position - Vector2.ONE,
						pixel_rect.size + Vector2.ONE * 2
					)
					var glow_color = Color(crt_green.r, crt_green.g, crt_green.b, glow_intensity * 0.3)
					draw_rect(glow_rect, glow_color)
			
			current_pixel += 1
		
		if current_pixel >= pixels_to_show:
			break
	
	# Draw scattered glitch pixels
	for glitch_pos in glitch_pixels:
		if randf() > 0.7: # Randomly show/hide glitch pixels
			var glitch_rect = Rect2(
				start_x + glitch_pos.x * pixel_size,
				start_y + glitch_pos.y * pixel_size,
				pixel_size,
				pixel_size
			)
			
			var glitch_color = crt_dark_green * flicker * randf_range(0.3, 0.8)
			draw_rect(glitch_rect, glitch_color)
	
	# Draw overall CRT screen effect
	draw_crt_overlay(viewport)

func draw_crt_overlay(viewport: Rect2):
	# Subtle screen curvature effect with gradient
	var gradient = Gradient.new()
	gradient.add_point(0.0, Color(0, 0, 0, 0.1))
	gradient.add_point(0.3, Color(0, 0, 0, 0.0))
	gradient.add_point(0.7, Color(0, 0, 0, 0.0))
	gradient.add_point(1.0, Color(0, 0, 0, 0.1))
	
	# Vignette effect
	for i in 20:
		var alpha = float(i) / 20.0 * 0.02
		var border_color = Color(0, 0, 0, alpha)
		var border_rect = Rect2(i, i, viewport.size.x - i * 2, viewport.size.y - i * 2)
		draw_rect(border_rect, Color.TRANSPARENT, false, 1.0)

func _process(_delta):
	# Continuously redraw for flicker effect
	queue_redraw()

# Public methods to control the effect
func set_scanline_intensity(intensity: float):
	scanline_intensity = clamp(intensity, 0.0, 1.0)

func set_glow_intensity(intensity: float):
	glow_intensity = clamp(intensity, 0.0, 2.0)

func set_pixel_size(size: float):
	base_pixel_size = max(size, 1.0)

# Called when animation completes (can be called by AnimationPlayer)
func on_animation_complete():
	animation_completed.emit()
	print("MainTitle: Animation completed signal emitted")
