shader_type spatial;

void vertex() {
	// Called for every vertex the material is visible on.
	VERTEX = vec3(10.0f, 10.0f, 10.0f);
}

void fragment() {
	// Called for every pixel the material is visible on.
	ALPHA = 0.0f;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
