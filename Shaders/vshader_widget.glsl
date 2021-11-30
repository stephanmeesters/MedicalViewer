#version 330
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform int renderMode;

void main()
{
    if(renderMode == 1)
    {
        gl_Position = vec4(aPos + aNormal * 0.003, 1.0) * model * view * projection;
    }
    else
    {
        gl_Position = vec4(aPos + aNormal * 0.05 , 1.0) * model * view * projection;
    }
}