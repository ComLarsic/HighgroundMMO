import { Gfx } from "./gfx";
import standardVert from "$lib/shaders/standard.vert.glsl";
import standardFrag from "$lib/shaders/standard.frag.glsl";
import { Shader } from "./shaders";
import { Sprite } from "./sprite";
import dummySprite from "$lib/assets/sprites/dummy.png";

/**
 * The renderer class is responsible for rendering the game world to the screen.
 */
export class Renderer {
  /// The standard shader for rendering sprites
  public static standardShader: Shader | null = null;

  private static _sprite: Sprite | null = null;

  /**
   * Initialize the renderer.
   */
  public static async init() {
    // Create the shader program.
    this.standardShader = new Shader(standardVert, standardFrag);
    // Create the sprite.
    this._sprite = await Sprite.load(this.standardShader, dummySprite);
  }

  /**
   * Render the game world to the screen.
   */
  public static render() {
    // Get the graphics context.
    const gl = Gfx.gl;

    // Clear the screen.
    gl.clearColor(0.0, 0.0, 0.0, 1.0);
    gl.clear(gl.COLOR_BUFFER_BIT);

    if (!this._sprite) {
      throw new Error("Sprite is not initialized.");
    }

    this._sprite?.draw({
      x: 0,
      y: 0,
      width: 128.0,
      height: 128.0,
    });
    this._sprite?.draw({
      x: 256,
      y: 256,
      width: 128.0,
      height: 128.0,
    });
    this._sprite?.draw({
      x: 180,
      y: 480,
      width: 128.0,
      height: 128.0,
    });
  }
}
