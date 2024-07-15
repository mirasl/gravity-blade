shader_type spatial;

//render_mode unshaded;

varying vec3 uv_world_pos;


//void vertex()
//{
//	VERTEX *= vec3(1.0 + sin(TIME*5.0)*0.05);
//}

void vertex() {
	// Vertex displacement for waves
	uv_world_pos = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
	if (uv_world_pos.x > 0.1)
	{
		VERTEX.x += sin(TIME);
	}
}

void fragment() {
	if (uv_world_pos.x > 0.1 && uv_world_pos.x < sin(TIME) + 0.1)
	{
		ALPHA = 0.0;
	}
}


//void vertex()
//{
//	VERTEX *= vec3(1.0 + 0.05*(sin(UV.x*100.0 + TIME)*sin(UV.x*100.0 + TIME) + 
//			sin(UV.y*100.0 + TIME)*sin(UV.y*100.0 + TIME)));
//}
