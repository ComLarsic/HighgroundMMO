/**
* The shader that handles drawing primative shapes. Should really only be done for debugging purposes.
*/
attribute vec3 position;
attribute vec4 color;

uniform mat4 screenSpaceMatrix;
uniform mat4 viewMatrix;

varying highp vec4 vColor;

void main() {
    gl_Position = screenSpaceMatrix * viewMatrix * vec4(position, 1.0);
    vColor = color;
}
