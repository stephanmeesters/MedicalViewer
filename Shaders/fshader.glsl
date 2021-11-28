#version 330

in vec3 FragPos;
in vec3 Normal;
in vec3 LightDir;

out vec4 fragColor;

struct Light {
    vec3 direction;
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

uniform float objectID;
uniform int normalRender;

void main()
{
    if(normalRender == 1)
    {
        // ambient
        vec3 colorc = vec3(texture(sampler, vec3(1-FragPos.x, FragPos.y, FragPos.z)).r);
        vec3 color =  (colorc * norm)*(1.0 - light.colorStrength) + light.color*light.colorStrength;
        vec3 ambient = light.ambient * color;

        // diffuse 
        vec3 normv = normalize(Normal);
        //normv += 10.0*vec3(texture(sampler, vec3(1-FragPos.x, FragPos.y, FragPos.z) + normv*0.1));
        //normv = normalize(normv);
        float colorc1 = texture(sampler, vec3(1-FragPos.x-normv.x*0.02, FragPos.y, FragPos.z)).r;
        float colorc2 = texture(sampler, vec3(1-FragPos.x, FragPos.y+normv.y*0.02, FragPos.z)).r;
        float colorc3 = texture(sampler, vec3(1-FragPos.x, FragPos.y, FragPos.z+normv.z*0.02)).r;
        normv = normv + normalize(colorc - vec3(colorc1, colorc2, colorc3))*0.2;
        normv = normalize(normv);

        vec3 lightDir = normalize(-light.direction)*0.00001 + normalize(LightDir);
        float diff = max(dot(normv, lightDir), 0.0);
        vec3 diffuse = light.diffuse * diff * color;

        // specular
        vec3 viewDir = normalize(viewPos);
        vec3 reflectDir = reflect(lightDir, normv);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
        vec3 specular = light.specular * spec * color;//vec3(texture(material.specular, TexCoords));

        vec3 result = ambient + diffuse + specular;
        fragColor = vec4(result, 1.0);// + vec4(normv, 1.0);
    }
    else
    {
        fragColor = vec4(vec3(objectID), 1.0);
    }
}