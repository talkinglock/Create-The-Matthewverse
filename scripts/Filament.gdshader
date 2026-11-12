shader_type spatial;

uniform float alpha;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	ALBEDO = vec3(1.0,0.4,0.1) * 50.0 * alpha + 0.1;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
