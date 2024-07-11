shader_type spatial;

//render_mode unshaded;

uniform sampler2D wave_texture;
uniform float wave_scale = 4.0;
uniform float height_scale = 1.0;
varying float wave_height;
varying vec3 uv_world_pos;
uniform float wave_speed : hint_range(0.0, 0.2) = 0.015;


//void vertex()
//{
//	VERTEX *= vec3(1.0 + sin(TIME*5.0)*0.05);
//}

void vertex() {
	// Vertex displacement for waves
	uv_world_pos = (WORLD_MATRIX * vec4(VERTEX, 1.0)).xyz;
	wave_height = sin(uv_world_pos.x + TIME)*sin(uv_world_pos.x + TIME*0.6) - 
			sin(uv_world_pos.y + TIME*0.2)*sin(uv_world_pos.y + TIME*0.7) - 
			sin(uv_world_pos.z + TIME*1.4)*sin(uv_world_pos.z + TIME*0.5);
	wave_height = 1.0 + wave_height*0.1;
//	wave_height = texture(wave_texture, uv_world_pos.xz / wave_scale + TIME * wave_speed).r;
	VERTEX *= wave_height * height_scale;
}


//void vertex()
//{
//	VERTEX *= vec3(1.0 + 0.05*(sin(UV.x*100.0 + TIME)*sin(UV.x*100.0 + TIME) + 
//			sin(UV.y*100.0 + TIME)*sin(UV.y*100.0 + TIME)));
//}
