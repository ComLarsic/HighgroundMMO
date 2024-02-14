/**
 * Standard vertex shader for drawing sprites.
*/
attribute vec3 position;
attribute vec2 uv;

uniform mat4 screenSpaceMatrix;
varying highp vec2 vUv;

void main() {
    gl_Position = screenSpaceMatrix * vec4(position, 1.0);
    vUv = uv;
}
