#version 330

in vec3 FragPos;
in vec3 Normal;
in vec3 LightDir;

out vec4 fragColor;

uniform sampler3D sampler;
uniform float norm;
uniform vec3 viewPos;

void main()
{
	vec3 bla = FragPos + Normal + LightDir;
	vec3 color = vec3(texture(sampler, vec3(1-FragPos.x, FragPos.y, FragPos.z)).r * norm);
	fragColor = vec4(color.rrr, 1.0) + 0.000001*vec4(bla, 1.0);
}