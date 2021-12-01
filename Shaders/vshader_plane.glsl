#version 330
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;

out vec3 FragPos;

uniform mat4 model;
uniform mat4 model2;
uniform mat4 view;
uniform mat4 projection;

uniform int renderMode;

void main()
{
    gl_Position = vec4(aPos, 1.0) * model * model2 * view * projection;
    if(renderMode == 1)
    {
        FragPos = vec3(vec4(aPos, 1.0) * model);
    }
    else
    {
        FragPos = vec3(1.0) + 0.00001*aNormal;
    }
    
}