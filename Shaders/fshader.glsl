#version 400

in vec3 FragPos;

out vec4 fragColor;

uniform sampler3D sampler;

vec4 color;

void main()
{
    color = texture(sampler, vec3(FragPos.x, FragPos.y, FragPos.z)).rrrr;
    fragColor = vec4(color.x, color.y, color.z, 1.0);
}
