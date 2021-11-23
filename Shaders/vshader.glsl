#version 400
layout(location = 0) in vec3 vertex;

out vec3 FragPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = vec4(vertex, 1.0) * model * view * projection;
    FragPos = vec3(vec4(vertex, 1.0) * model);
}