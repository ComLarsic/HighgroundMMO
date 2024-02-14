import { Gfx } from "./gfx";
import type { Shader } from "./shaders";

/**
 * Represents the dimensions of a sprite.
 */
export type SpriteBounds = {
  x: number;
  y: number;
  width: number;
  height: number;
};

/**
 * Represents a sprite.
 */
export class Sprite {
  /// The sprite texture
  private _texture: WebGLTexture;
  /// The sprite shader
  private _shader: Shader | null = null;

  /// The vertex array
  private _vertexArray: WebGLVertexArrayObject;
  /// The vertex buffer
  private _vertexBuffer: WebGLBuffer;
  /// The index buffer
  private _indexBuffer: WebGLBuffer;

  /// The sprite bounds. Cached for performance.
  private _bounds: SpriteBounds = {
    x: 0,
    y: 0,
    width: 0,
    height: 0,
  };

  /// The color of the sprite.
  private _color: Float32Array = new Float32Array([1.0, 1.0, 1.0, 1.0]);

  /**
   * Set the sprite color.
   */
  public set color(value: { r: number; g: number; b: number; a: number }) {
    this._color = new Float32Array([value.r, value.g, value.b, value.a]);
  }

  /**
   * Get the sprite color.
   */
  public get color(): { r: number; g: number; b: number; a: number } {
    return {
      r: this._color[0],
      g: this._color[1],
      b: this._color[2],
      a: this._color[3],
    };
  }

  /**
   * Create a new sprite.
   * @param texture The sprite texture.
   * @param shader The sprite shader.
   */
  constructor(shader: Shader, texture: WebGLTexture) {
    this._texture = texture;
    this._shader = shader;

    // Create the vertex array.
    const vertexArray = Gfx.gl.createVertexArray();
    if (vertexArray === null) {
      throw new Error("Failed to create vertex array.");
    }
    this._vertexArray = vertexArray;

    // Create the vertex buffer.
    const vertexBuffer = Gfx.gl.createBuffer();
    if (vertexBuffer === null) {
      throw new Error("Failed to create vertex buffer.");
    }
    this._vertexBuffer = vertexBuffer;

    // Create the index buffer.
    const indexBuffer = Gfx.gl.createBuffer();
    if (indexBuffer === null) {
      throw new Error("Failed to create index buffer.");
    }
    this._indexBuffer = indexBuffer;
  }

  /**
   * Load a sprite from a file.
   */
  public static async load(shader: Shader, url: string): Promise<Sprite> {
    const texture = await Gfx.loadTexture(url);
    return new Sprite(shader, texture);
  }

  /**
   * Draw the sprite.
   */
  public draw(bounds: SpriteBounds): void {
    const gl = Gfx.gl;

    // Set the sprite bounds. This is cached for performance.
    if (
      bounds.x !== this._bounds.x ||
      bounds.y !== this._bounds.y ||
      bounds.width !== this._bounds.width ||
      bounds.height !== this._bounds.height
    ) {
      this.setBounds(bounds);
    }

    if (this._shader === null) {
      throw new Error("Sprite shader is not initialized.");
    }

    // Use the shader.
    this._shader.use();
    // Set the screen space matri.
    this._shader.setUniform(
      "screenSpaceMatrix",
      "Matrix4fv",
      Gfx.screenSpaceMatrix
    );
    // Set the color.
    this._shader.setUniform("uColor", "4fv", this._color);

    // Bind the texture.
    if (this._texture !== null) {
      gl.activeTexture(gl.TEXTURE0);
      gl.bindTexture(gl.TEXTURE_2D, this._texture);
      this._shader.setUniform("texture", "1i", 0);
    }

    // Bind the vertex array.
    gl.bindVertexArray(this._vertexArray);

    // Draw the sprite.
    gl.drawElements(gl.TRIANGLES, 6, gl.UNSIGNED_SHORT, 0);
  }

  /**
   * Set the sprite bounds.
   * @param bounds The sprite bounds.
   */
  private setBounds(bounds: SpriteBounds): void {
    this._bounds = bounds;

    const gl = Gfx.gl;

    // Create the vertices.
    // Vec3 position, Vec2 texCoord
    const vertices = new Float32Array([
      // Top left
      bounds.x,
      bounds.y + bounds.height,
      0.0,
      0.0,
      1.0,
      // Bottom left
      bounds.x,
      bounds.y,
      0.0,
      0.0,
      0.0,
      // Bottom right
      bounds.x + bounds.width,
      bounds.y,
      0.0,
      1.0,
      0.0,
      // Top right
      bounds.x + bounds.width,
      bounds.y + bounds.height,
      0.0,
      1.0,
      1.0,
    ]);

    // Create the indices.
    const indices = new Uint16Array([0, 1, 2, 0, 2, 3]);

    // Bind the vertex array.
    gl.bindVertexArray(this._vertexArray);

    // Bind the vertex buffer.
    gl.bindBuffer(gl.ARRAY_BUFFER, this._vertexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

    // Bind the index buffer.
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, this._indexBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

    // Set the vertex attributes.
    gl.vertexAttribPointer(
      0,
      3,
      gl.FLOAT,
      false,
      5 * Float32Array.BYTES_PER_ELEMENT,
      0
    );
    gl.enableVertexAttribArray(0);
    gl.vertexAttribPointer(
      1,
      2,
      gl.FLOAT,
      false,
      5 * Float32Array.BYTES_PER_ELEMENT,
      3 * Float32Array.BYTES_PER_ELEMENT
    );
    gl.enableVertexAttribArray(1);
  }
}
