import { Gfx } from "./gfx";
import type { Shader } from "./shaders";
import { Texture } from "./texture";

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
  private _texture: Texture;
  /// The sprite shader
  private _shader: Shader | null = null;

  /// The vertex array
  private _vertexArray: WebGLVertexArrayObject;
  /// The vertex buffer
  private _vertexBuffer: WebGLBuffer;
  /// The index buffer
  private _indexBuffer: WebGLBuffer;

  /// The sprite bounds. Cached for performance.
  private _destBounds: SpriteBounds = {
    x: 0,
    y: 0,
    width: 0,
    height: 0,
  };

  /// The source bounds of the sprite.
  private _sourceBounds: SpriteBounds = {
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
   * Get the sprite texture.
   */
  public get texture(): Texture {
    return this._texture;
  }

  /**
   * Create a new sprite.
   * @param texture The sprite texture.
   * @param shader The sprite shader.
   */
  constructor(shader: Shader, texture: Texture) {
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
    const texture = await Texture.load(url);
    return new Sprite(shader, texture);
  }

  /**
   * Draw the sprite at the given position.
   */
  public draw(x: number, y: number, width: number, height: number): void {
    this.drawBounded(
      { x: 0, y: 0, width: this._texture.width, height: this._texture.height },
      { x, y, width, height }
    );
  }

  /**
   * Draw the sprite with the given bounds.
   */
  public drawBounded(source: SpriteBounds, dest: SpriteBounds): void {
    const gl = Gfx.gl;

    /// Compare two sprite bounds.
    const compareBounds = (a: SpriteBounds, b: SpriteBounds): boolean => {
      return (
        a.x === b.x &&
        a.y === b.y &&
        a.width === b.width &&
        a.height === b.height
      );
    };

    // Set the sprite bounds. This is cached for performance.
    if (
      !compareBounds(this._destBounds, dest) ||
      !compareBounds(this._sourceBounds, source)
    ) {
      this.setBounds(dest, source);
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
      gl.bindTexture(gl.TEXTURE_2D, this._texture.texture);
      this._shader.setUniform("texture", "1i", 0);
    }

    // Bind the vertex array.
    gl.bindVertexArray(this._vertexArray);

    // Draw the sprite.
    gl.drawElements(gl.TRIANGLES, 6, gl.UNSIGNED_SHORT, 0);
  }

  /**
   * Set the sprite bounds.
   * @param dest The sprite destination bounds.
   * @param source The sprite source bounds.
   */
  private setBounds(dest: SpriteBounds, source: SpriteBounds): void {
    this._destBounds = dest;
    this._sourceBounds = source;

    const gl = Gfx.gl;

    // Destroy the vertex array if it exists.
    if (gl.isVertexArray(this._vertexArray)) {
      gl.deleteVertexArray(this._vertexArray);
    }
    if (gl.isBuffer(this._vertexBuffer)) {
      gl.deleteBuffer(this._vertexBuffer);
    }
    if (gl.isBuffer(this._indexBuffer)) {
      gl.deleteBuffer(this._indexBuffer);
    }

    // Create the vertex array.
    const vertexArray = gl.createVertexArray();
    if (vertexArray === null) {
      throw new Error("Failed to create vertex array.");
    }
    this._vertexArray = vertexArray;

    // Create the vertex buffer.
    const vertexBuffer = gl.createBuffer();
    if (vertexBuffer === null) {
      throw new Error("Failed to create vertex buffer.");
    }
    this._vertexBuffer = vertexBuffer;

    // Create the index buffer.
    const indexBuffer = gl.createBuffer();
    if (indexBuffer === null) {
      throw new Error("Failed to create index buffer.");
    }
    this._indexBuffer = indexBuffer;

    // Create the vertices.
    // Vec3 position, Vec2 texCoord
    const uv = {
      x: source.x / this._texture.width || 0,
      y: source.y / this._texture.height || 0,
      width: source.width / this._texture.width || 0,
      height: source.height / this._texture.height || 0,
    };

    // (Use standard UV for now)
    const vertices = new Float32Array([
      dest.x,
      dest.y,
      0.0,
      uv.x,
      uv.y,
      dest.x + dest.width,
      dest.y,
      0.0,
      uv.x + uv.width,
      uv.y,
      dest.x + dest.width,
      dest.y + dest.height,
      0.0,
      uv.x + uv.width,
      uv.y + uv.height,
      dest.x,
      dest.y + dest.height,
      0.0,
      uv.x,
      uv.y + uv.height,
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
