#version 330
layout(location = 0) in vec4 vertex;
layout(location = 1) in vec4 texcoord;

out vec4 texCoordSampled;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    texCoordSampled = texcoord;
    gl_Position = vertex * view * projection;
}