import { Gfx } from "./gfx";

/// Represents a uniform type.
export type UniformType =
  | "1f"
  | "1fv"
  | "1i"
  | "1iv"
  | "2f"
  | "2fv"
  | "2i"
  | "2iv"
  | "3f"
  | "3fv"
  | "3i"
  | "3iv"
  | "4f"
  | "4fv"
  | "4i"
  | "4iv"
  | "Matrix2fv"
  | "Matrix3fv"
  | "Matrix4fv";

/**
 * Represents a shader.
 */
export class Shader {
  /// The shader program.
  private _program: WebGLProgram | null = null;

  /// Cache of uniform locations.
  private _uniforms: { [name: string]: WebGLUniformLocation | null } = {};

  /**
   * Create a new shader.
   * @param vertex The vertex shader.
   * @param fragment The fragment shader.
   */
  constructor(vertex: string, fragment: string) {
    const gl = Gfx.gl;
    const vertexShader = this._createShader(gl.VERTEX_SHADER, vertex);
    const fragmentShader = this._createShader(gl.FRAGMENT_SHADER, fragment);

    this._program = gl.createProgram();
    if (!this._program) {
      throw new Error("Failed to create shader program.");
    }

    gl.attachShader(this._program, vertexShader);
    gl.attachShader(this._program, fragmentShader);
    gl.linkProgram(this._program);

    if (!gl.getProgramParameter(this._program, gl.LINK_STATUS)) {
      throw new Error(
        "Failed to link shader program: " + gl.getProgramInfoLog(this._program)
      );
    }
  }

  /**
   * Use the shader.
   */

  public use(): void {
    const gl = Gfx.gl;
    if (!this._program) {
      throw new Error("Shader program is not initialized.");
    }
    gl.useProgram(this._program);
  }

  /**
   * Create a shader.
   */
  private _createShader(type: number, source: string): WebGLShader {
    const gl = Gfx.gl;
    const shader = gl.createShader(type);
    if (!shader) {
      throw new Error("Failed to create shader.");
    }

    gl.shaderSource(shader, source);
    gl.compileShader(shader);

    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
      throw new Error(
        "Failed to compile shader: " + gl.getShaderInfoLog(shader)
      );
    }

    return shader;
  }

  /**
   * Get the uniform location.
   */
  public getUniformLocation(name: string): WebGLUniformLocation | null {
    const gl = Gfx.gl;
    if (!this._program) {
      throw new Error("Shader program is not initialized.");
    }
    if (this._uniforms[name] === undefined) {
      this._uniforms[name] = gl.getUniformLocation(this._program, name);
    }
    return this._uniforms[name];
  }

  /**
   * Set a uniform.
   */
  public setUniform(name: string, type: UniformType, value: any): void {
    const gl = Gfx.gl;
    const location = this.getUniformLocation(name);
    if (location === null) {
      throw new Error("Uniform location is not found.");
    }
    switch (type) {
      case "1f":
        gl.uniform1f(location, value as number);
        break;
      case "1fv":
        gl.uniform1fv(location, value as Float32Array);
        break;
      case "1i":
        gl.uniform1i(location, value as number);
        break;
      case "1iv":
        gl.uniform1iv(location, value as Int32Array);
        break;
      case "2f":
        gl.uniform2f(location, value[0], value[1]);
        break;
      case "2fv":
        gl.uniform2fv(location, value as Float32Array);
        break;
      case "2i":
        gl.uniform2i(location, value[0], value[1]);
        break;
      case "2iv":
        gl.uniform2iv(location, value as Int32Array);
        break;
      case "3f":
        gl.uniform3f(location, value[0], value[1], value[2]);
        break;
      case "3fv":
        gl.uniform3fv(location, value as Float32Array);
        break;
      case "3i":
        gl.uniform3i(location, value[0], value[1], value[2]);
        break;
      case "3iv":
        gl.uniform3iv(location, value as Int32Array);
        break;
      case "4f":
        gl.uniform4f(location, value[0], value[1], value[2], value[3]);
        break;
      case "4fv":
        gl.uniform4fv(location, value as Float32Array);
        break;
      case "4i":
        gl.uniform4i(location, value[0], value[1], value[2], value[3]);
        break;
      case "4iv":
        gl.uniform4iv(location, value as Int32Array);
        break;
      case "Matrix2fv":
        gl.uniformMatrix2fv(location, false, value as Float32Array);
        break;
      case "Matrix3fv":
        gl.uniformMatrix3fv(location, false, value as Float32Array);
        break;
      case "Matrix4fv":
        gl.uniformMatrix4fv(location, false, value as Float32Array);
        break;
    }
  }
}
