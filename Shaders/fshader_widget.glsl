#version 330

out vec4 fragColor;

uniform vec3 color;
uniform float objectID;
uniform int renderMode;

void main()
{
	if(renderMode == 1)
	{
		fragColor = vec4(color, 1.0);
	}
	else
	{
		fragColor = vec4(vec3(objectID), 1.0);
	}
	
}