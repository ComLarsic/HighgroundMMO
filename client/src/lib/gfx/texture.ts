import { Gfx } from "./gfx";

/**
 * Represents a texture.
 */
export class Texture {
  /// The texture
  private _texture: WebGLTexture;

  /// The width of the texture.
  private _width: number;
  /// The height of the texture.
  private _height: number;

  /// A texture with 1 white.
  /// Mostly used for debug drawing.
  private static _whiteTexture: Texture | null = null;

  /**
   * Get a white texture.
   * @returns The white texture.
   */
  public static get whiteTexture(): Texture {
    if (!this._whiteTexture) {
      const gl = Gfx.gl;
      const texture = gl.createTexture();
      if (!texture) {
        throw new Error("Failed to create texture.");
      }

      gl.bindTexture(gl.TEXTURE_2D, texture);
      gl.texImage2D(
        gl.TEXTURE_2D,
        0,
        gl.RGBA,
        1,
        1,
        0,
        gl.RGBA,
        gl.UNSIGNED_BYTE,
        new Uint8Array([255, 255, 255, 255])
      );
      this._whiteTexture = new Texture(texture, 1, 1);
    }
    return this._whiteTexture;
  }

  /**
   * Get the texture.
   */
  public get texture(): WebGLTexture {
    return this._texture;
  }

  /**
   * Get the width of the texture.
   */
  public get width(): number {
    return this._width;
  }

  /**
   * Get the height of the texture.
   */
  public get height(): number {
    return this._height;
  }

  /**
   * Create a new texture from a webgl texture.
   * @param texture The webgl texture.
   * @param width The width of the texture.
   * @param height The height of the texture.
   * @returns The texture.
   */
  constructor(texture: WebGLTexture, width: number, height: number) {
    this._texture = texture;
    this._width = width;
    this._height = height;
  }

  /**
   * Create a new texture.
   * @param image The image to create the texture from.
   */
  public static fromImage(image: HTMLImageElement): Texture {
    const gl = Gfx.gl;
    const texture = gl.createTexture();
    if (!texture) {
      throw new Error("Failed to create texture.");
    }

    const width = image.width;
    const height = image.height;

    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, image);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);

    return new Texture(texture, width, height);
  }

  /**
   * Load a texture.
   * @param gl The graphics context.
   * @param url The URL of the texture to load.
   * @returns The texture.
   * @throws An error if the texture could not be loaded.
   */
  public static async load(url: string): Promise<Texture> {
    const image = new Image();
    image.src = url;

    return new Promise((resolve, reject) => {
      image.onload = () => {
        resolve(Texture.fromImage(image));
      };
      image.onerror = () => {
        reject(new Error(`Failed to load texture ${url}.`));
      };
    });
  }
}
