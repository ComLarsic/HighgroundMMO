/**
 * Standard fragment shader for drawing sprites.
*/
precision mediump float;

varying highp vec2 vUv;

uniform vec4 uColor;
uniform sampler2D texture;

void main() {
    gl_FragColor = texture2D(texture, vUv) * uColor;
}
