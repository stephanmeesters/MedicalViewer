#version 330

in vec3 FragPos;
in vec3 Normal;

out vec4 fragColor;

void main()
{
	vec3 bla = FragPos + Normal;
	fragColor = vec4(0.0) + 0.000001*vec4(bla, 1.0);
}