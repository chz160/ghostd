shader_type canvas_item;
render_mode unshaded, skip_vertex_transform;

// Scanline effects
uniform float scanline_intensity : hint_range(0.0, 0.5, 0.01) = 0.1;
uniform float scanline_density : hint_range(100.0, 800.0, 50.0) = 400.0;
uniform float scanline_brightness : hint_range(0.5, 1.0, 0.05) = 0.9;

// Blur amount (simulates CRT phosphor blur)
uniform float blur_amount : hint_range(0.0, 1.0, 0.1) = 0.2;

// Color adjustments
uniform float brightness : hint_range(0.5, 1.5, 0.05) = 1.0;

uniform sampler2D screen_texture: hint_screen_texture, filter_linear_mipmap;

void fragment() {
	// Convert to centered coordinates (-1 to 1)
	vec2 centered_uv = SCREEN_UV * 2.0 - 1.0;

	// Generate scanlines
	// scanline_brightness is the center value, intensity is the variation
	vec3 scanlines = vec3(sin(2.0 * centered_uv.y * scanline_density) * scanline_intensity + scanline_brightness);

	// Sample the screen with slight blur
	vec3 screen_color = textureLod(screen_texture, (centered_uv + 1.0) / 2.0, blur_amount).rgb;

	// Apply all effects
	screen_color *= scanlines;     // Scanline effect
	screen_color *= brightness;    // Overall brightness

	// Ensure we don't go below 0
	screen_color = max(vec3(0.0), screen_color);

	COLOR = vec4(screen_color, 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
