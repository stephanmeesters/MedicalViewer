#version 330

in vec3 FragPos;

out vec4 fragColor;

uniform sampler3D sampler;
uniform float norm;

void main()
{
        // ambient
        vec3 color = vec3(texture(sampler, vec3(FragPos.x, FragPos.y, FragPos.z)).r * norm);
        fragColor = vec4(color*0.00000001 + vec3(1.0, 1.0, 1.0), 1.0);
}