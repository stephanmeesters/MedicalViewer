#version 400

in vec3 FragPos;

out vec4 fragColor;

uniform sampler3D sampler;
uniform float norm;

float color;

void main()
{
    color = texture(sampler, vec3(FragPos.x, FragPos.y, FragPos.z)).r * norm;
    fragColor = vec4(color, color, color, 1.0);
}
