/**
* The shader that handles drawing primative shapes. Should really only be done for debugging purposes.
*/
precision mediump float;

varying highp vec4 vColor;

void main() {
    gl_FragColor = vColor;
}
