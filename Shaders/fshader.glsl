#version 330
in vec4 texCoordSampled;
out highp vec4 fragColor;
//uniform sampler2D sampler;
uniform sampler3D sampler;
uniform float slice;
vec4 color;
void main() {
    if(texCoordSampled.w == 1.0f)
    {
        color = texture(sampler, vec3(texCoordSampled.x, texCoordSampled.y, slice)).rrrr;
    }
    else if(texCoordSampled.w == 2.0f)
    {
        color = texture(sampler, vec3(texCoordSampled.x, slice, texCoordSampled.z)).rrrr;
    }
    else if(texCoordSampled.w == 3.0f)
    {
        color = texture(sampler, vec3(slice, texCoordSampled.y, texCoordSampled.z)).rrrr;
    }
    else if(texCoordSampled.w == 4.0f)
    {
        color = texture(sampler, vec3(1.0-slice, texCoordSampled.y, texCoordSampled.z)).rrrr;
    }
    else
    {
        color = texture(sampler, texCoordSampled.xyz).rrrr;
    }

   //color = texture(sampler, vec3(texCoordSampled.x, texCoordSampled.y, slice)).rrrr;
   fragColor = vec4(color.x, color.y, color.z, 1.0);//vec4(texCoordSampled.x, texCoordSampled.y, 1.0, 1.0);
}
