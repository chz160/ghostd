shader_type canvas_item;
render_mode unshaded, skip_vertex_transform;

// CRT screen curvature
uniform float curvature : hint_range(0.0, 20.0, 0.5) = 7.0;

// Scanline effects
uniform float scanline_intensity : hint_range(0.0, 0.5, 0.01) = 0.1;
uniform float scanline_density : hint_range(100.0, 800.0, 50.0) = 400.0;
uniform float scanline_brightness : hint_range(0.5, 1.0, 0.05) = 0.9;

// Vignette (corner darkening)
uniform float vignette_strength : hint_range(0.0, 3.0, 0.1) = 1.5;
uniform float vignette_falloff : hint_range(2.0, 8.0, 0.5) = 4.0;

// Blur amount (simulates CRT phosphor blur)
uniform float blur_amount : hint_range(0.0, 1.0, 0.1) = 0.2;

// Color adjustments
uniform float brightness : hint_range(0.5, 1.5, 0.05) = 1.0;
uniform vec3 tint_color : source_color = vec3(0.2, 1.0, 0.2);
uniform float tint_strength : hint_range(0.0, 1.0, 0.05) = 0.1;

uniform sampler2D screen_texture: hint_screen_texture, filter_linear_mipmap;

void fragment() {
	// Convert to centered coordinates (-1 to 1)
	vec2 centered_uv = SCREEN_UV * 2.0 - 1.0;

	// Apply barrel distortion for CRT curve
	vec2 uv_offset = centered_uv.yx / curvature;
	vec2 warped_uv = centered_uv + centered_uv * uv_offset * uv_offset;

	// Create black border outside curved area
	vec3 cutoff = vec3(step(abs(warped_uv.x), 1.0) * step(abs(warped_uv.y), 1.0));

	// Generate scanlines
	// scanline_brightness is the center value, intensity is the variation
	vec3 scanlines = vec3(sin(2.0 * warped_uv.y * scanline_density) * scanline_intensity + scanline_brightness);

	// Create vignette effect
	vec3 vignette = vec3(length(pow(abs(centered_uv), vec2(vignette_falloff)) / 3.0));

	// Sample the screen with slight blur
	vec3 screen_color = textureLod(screen_texture, (warped_uv + 1.0) / 2.0, blur_amount).rgb;

	//screen_color -= vignette * vignette_strength;

	// Apply all effects
	screen_color *= cutoff;        // Black border
	screen_color *= scanlines;     // Scanline effect
	screen_color -= vignette * vignette_strength;  // Vignette darkening
	screen_color *= brightness;    // Overall brightness

	// Apply subtle color tint (classic green terminal look)
	screen_color = mix(screen_color, screen_color * tint_color, tint_strength);

	// Ensure we don't go below 0
	screen_color = max(vec3(0.0), screen_color);

	COLOR = vec4(screen_color, 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
