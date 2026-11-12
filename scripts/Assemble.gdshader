shader_type spatial;

void vertex() {
	vec3 dir = normalize(VERTEX);
    VERTEX = (dir - vec3(0.0f, -0.8f, 0.0f)) * 1.0f;
}

void fragment() {

}
