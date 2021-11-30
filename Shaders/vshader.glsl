#version 330
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;

out vec3 FragPos;
out vec3 Normal;
out vec3 LightDir;

uniform mat4 model;
uniform mat4 model2;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 viewPos;
uniform int renderMode;

void main()
{
    if(renderMode == 3)
    {
        gl_Position = vec4(aPos - normalize(aNormal), 1.0) * model * model2 * view * projection;
        FragPos = vec3(1.0);
        Normal = vec3(1.0);
        LightDir = vec3(1.0); 
    }
    else
    {
        gl_Position = vec4(aPos, 1.0) * model * model2 * view * projection;
        FragPos = vec3(vec4(aPos, 1.0) * model);
        Normal = aNormal * mat3(transpose(inverse(model))) * mat3(transpose(inverse(model2)));
        LightDir = vec3(gl_Position.x, gl_Position.y, gl_Position.z) - normalize(viewPos)*30; 
    }
    
}