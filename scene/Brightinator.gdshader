shader_type spatial;

uniform float alphaVal;
void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	EMISSION = vec3(1.0,1.0,1.0) * 10.0f;
	ALPHA = alphaVal;
}
