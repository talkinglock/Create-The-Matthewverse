shader_type spatial;

uniform vec3 color;
uniform float alphaVal;
void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	ALBEDO = color;
	EMISSION = color * alphaVal;
}
