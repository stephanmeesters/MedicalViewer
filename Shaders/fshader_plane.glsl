#version 330

in vec3 FragPos;

out vec4 fragColor;

uniform sampler3D sampler;
uniform float norm;
uniform vec3 viewPos;

uniform float objectID;
uniform vec3 outlineColor;
uniform int renderMode;

void main()
{
	if(renderMode == 1)
    {
	    vec3 color = vec3(texture(sampler, vec3(FragPos.x, FragPos.y, FragPos.z)).r * norm);
	    fragColor = vec4(color, 1.0);
    }
    else if(renderMode == 2)
    {
        fragColor = vec4(vec3(objectID), 1.0);
    }
    else if(renderMode == 3)
    {
        fragColor = vec4(outlineColor, 1.0);
    }
}