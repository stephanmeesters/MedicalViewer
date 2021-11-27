#version 330

in vec3 FragPos;
in vec3 Normal;

out vec4 fragColor;

struct Light {
    vec3 direction;
    float base;
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    vec3 color;
    float colorStrength;
};
uniform Light light;
uniform sampler3D sampler;
uniform float norm;
uniform vec3 viewPos;



void main()
{

        // ambient
        vec3 color = vec3(texture(sampler, vec3(FragPos.x, FragPos.y, FragPos.z)).r * norm)*(1.0 - light.colorStrength) + light.color*light.colorStrength;
        vec3 ambient = light.ambient * color + vec3(light.base);

        // diffuse 
        vec3 norm = normalize(Normal);
        vec3 lightDir = normalize(-light.direction)*0.00001 + normalize(viewPos);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = light.diffuse * diff * color;

        // specular
        vec3 viewDir = normalize(viewPos);
        vec3 reflectDir = reflect(-lightDir, norm);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
        vec3 specular = light.specular * spec * color;//vec3(texture(material.specular, TexCoords));

        vec3 result = ambient + diffuse + specular;
        fragColor = vec4(result, 1.0);


    
}