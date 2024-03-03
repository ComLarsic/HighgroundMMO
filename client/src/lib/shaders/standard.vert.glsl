/**
 * Standard vertex shader for drawing sprites.
*/
attribute vec3 position;
attribute vec2 uv;

uniform mat4 screenSpaceMatrix;
uniform mat4 viewMatrix;

varying highp vec2 vUv;

void main() {
    gl_Position = screenSpaceMatrix * viewMatrix * vec4(position, 1.0);
    vUv = uv;
}
