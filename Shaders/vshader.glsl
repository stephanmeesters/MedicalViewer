#version 330
layout(location = 0) in vec4 vertex;
layout(location = 1) in vec4 texcoord;
out vec4 texCoordSampled;
uniform mat4 projMatrix;
uniform mat4 camMatrix;
uniform mat4 worldMatrix;
uniform mat4 myMatrix;
//uniform sampler2D sampler;
void main() {
   //ivec2 pos = ivec2(16,18);
   //vec2 t = vec2(float(-16 + pos.x) * 0.8, float(-18 + pos.y) * 0.6);
   // vec2 t = vec2(0.0, 0.0);
   //mat4 wm = myMatrix * mat4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, t.x, t.y, 0, 1) * worldMatrix;
    texCoordSampled = texcoord;
   gl_Position = vertex;//projMatrix * camMatrix * wm * vertex;
}
