shader_type canvas_item;

void fragment()
{
	vec2 tiledUV = UV;
	float dotInterval = 1.0;
	while (tiledUV.x > dotInterval)
	{
		tiledUV.x -= dotInterval;
	}
	tiledUV -= 0.5;
	if (tiledUV.x*tiledUV.x + tiledUV.y*tiledUV.y > 0.1)
	{
		COLOR.a = 0.0;
	}
//	COLOR.rgb = vec3(grayscale, grayscale, grayscale);
}