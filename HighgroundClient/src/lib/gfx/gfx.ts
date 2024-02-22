import { mat4 } from "math.gl";

/**
 * The Gfx class is the main class for the graphics engine.
 */
export class Gfx {
  /// The canvas element.
  private static _canvas: HTMLCanvasElement | null = null;
  /// The graphics context.
  private static _gl: WebGL2RenderingContext | null = null;

  /// The screen space projection matrix.
  private static _screenSpaceMatrix: Float32Array = new Float32Array(16);

  /// The width of the canvas.
  public static get width(): number {
    if (!this._canvas) {
      throw new Error("Canvas is not initialized.");
    }
    return this._canvas.width;
  }

  /// The height of the canvas.
  public static get height(): number {
    if (!this._canvas) {
      throw new Error("Canvas is not initialized.");
    }
    return this._canvas.height;
  }

  /// The canvas element.
  public static init(canvas: HTMLCanvasElement): void {
    this._canvas = canvas;
    this._gl = canvas.getContext("webgl2");

    if (!this._gl) {
      throw new Error("WebGL2 is not supported.");
    }

    // Enable transparency.
    this._gl.enable(this._gl.BLEND);
    this._gl.blendFunc(this._gl.SRC_ALPHA, this._gl.ONE_MINUS_SRC_ALPHA);
  }

  /**
   * Resize the canvas to fit the window.
   * @throws An error if the canvas is not initialized.
   */
  public static resize() {
    if (!this._canvas) {
      throw new Error("Canvas is not initialized.");
    }
    // Get the width and height of the window.
    const width =
      document.documentElement.clientWidth || document.body.clientWidth;
    const height =
      document.documentElement.clientHeight || document.body.clientHeight;

    // Resize the canvas.
    this._canvas.width = width;
    this._canvas.height = height;

    // Create the projection matrix.
    const projectionMatrix = mat4.create();
    mat4.ortho(projectionMatrix, 0, width, height, 0, -1, 1);
    this._screenSpaceMatrix = new Float32Array(projectionMatrix);

    Gfx.gl.viewport(0, 0, width, height);
  }

  /**
   * Get the graphics context.
   */
  public static get gl(): WebGL2RenderingContext {
    if (!this._gl) {
      throw new Error("WebGL2 is not supported.");
    }
    return this._gl;
  }

  /**
   * Get the screen space projection matrix.
   */
  public static get screenSpaceMatrix(): Float32Array {
    return this._screenSpaceMatrix;
  }

  /**
   * Clear the screen.
   */
  public static clear() {
    const gl = Gfx.gl;
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT);
  }
}
