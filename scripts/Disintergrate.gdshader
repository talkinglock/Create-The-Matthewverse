shader_type spatial;
varying vec3 local_pos;

uniform float fadeB;
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(12.9898,78.233))) * 43758.5453123); 
}

float noise(vec2 x) {
    vec2 p = floor(x);
    vec2 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    vec2 a = vec2(1.0, 0.0);
    return mix(mix(hash(p + a.yy), hash(p + a.xy), f.x), mix(hash(p + a.yx), hash(p + a.xx), f.x), f.y);
}
void vertex() {
	// Called for every vertex the material is visible on.
	local_pos = VERTEX;
}
void fragment()
{
	float dist = length(local_pos.xyz) + noise((local_pos.xy * 3.0f));
	float fade = 1.0 - smoothstep(0.0,fadeB,dist);
	if (fade <= 0.7 && fade > 0.0f)
	{
		EMISSION = vec3(0.0f,0.5f,1.0f) * 100.0f;
		ALPHA = 1.0f;
	}
	else
	{
		EMISSION = vec3(0.5f,0.5f,0.5f) * 10.0f;
		ALPHA = fade;
	}
	
}


//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
