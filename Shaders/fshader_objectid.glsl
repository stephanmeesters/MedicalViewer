#version 330

out vec4 fragColor;

uniform float object_id;

void main()
{
    fragColor = vec4(vec3(object_id), 1.0);
}