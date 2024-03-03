import { Gfx } from "./gfx";
import standardVert from "$lib/shaders/standard.vert.glsl";
import standardFrag from "$lib/shaders/standard.frag.glsl";
import { Shader } from "./shaders";
import { Sprite } from "./sprite";
import { BitMapFont } from "./font";

import dummySprite from "$lib/assets/sprites/dummy.png";
import dummyFont from "$lib/assets/fonts/pressstart2p_outlined.png";
import { PhysicsObjectType, World } from "$lib/game/world";
import { Player } from "$lib/networking/player";
import { PlayerList } from "$lib/networking/player_list";
import { Camera } from "./camera";
import { Texture } from "./texture";
/**
 * The renderer class is responsible for rendering the game world to the screen.
 */
export class Renderer {
  /// The standard shader for rendering sprites
  public static standardShader: Shader | null = null;

  private static _sprite: Sprite | null = null;
  private static _font: BitMapFont | null = null;
  private static _primativeSprite: Sprite | null = null;

  /// The flag for if the server world should be rendered.
  public static renderServerWorld: boolean = false;

  /// The flag for if the collision boxes should be rendered.
  public static renderCollisionBoxes: boolean = false;

  /**
   * Initialize the renderer.
   */
  public static async init() {
    // Create the shader program.
    this.standardShader = new Shader(standardVert, standardFrag);
    // Create the sprite.
    this._sprite = await Sprite.load(this.standardShader, dummySprite);
    // Create the font.
    this._font = await BitMapFont.load(
      this.standardShader,
      dummyFont,
      30,
      30,
      " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
    );
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

    // Set the view matrix
    const viewMatrix = Camera.getViewMatrix();
    this.standardShader?.use();
    this.standardShader?.setUniform("viewMatrix", "Matrix4fv", viewMatrix);

    if (!this._sprite) {
      throw new Error("Sprite is not initialized.");
    }

    // For each entity in the world, render it.
    for (const entity of World.entities.values()) {
      // If the entity is a player, use the client's position.
      const position =
        entity.playerId === Player.id
          ? Player.physicsData.position
          : entity.physicsObject.position;

      // If the entity has a player ID, add its username to the screen.
      if (entity.playerId === null) {
        this.primativeSprite.color = {
          r: 1.0,
          g: 1.0,
          b: 1.0,
          a: 1,
        };
        if (entity.collider !== null) {
          this.primativeSprite.draw(
            position.X,
            position.Y,
            entity.collider.size.X,
            entity.collider.size.Y
          );
        }
      } else {
        this._sprite.color = {
          r: 1.0,
          g: 1.0,
          b: 1.0,
          a: 1.0,
        };
        this._sprite.draw(position.X, position.Y, 96, 96);
      }
    }

    // Render the player usernames.
    for (const entity of World.entities.values()) {
      if (entity.playerId === null) continue;
      if (entity.playerId === Player.id) continue;
      const player = PlayerList.get(entity.playerId);
      if (player !== undefined) {
        if (!this._font) {
          throw new Error("Font is not initialized.");
        }

        const player = PlayerList.get(entity.playerId);
        let name = "Unknown";
        if (player) {
          name = player.username;
        }

        // Set the color of the text.
        const color = { r: 1, g: 1, b: 1, a: 1 };
        const width = this._font?.calculateWidth(name, 24) || 0;
        if (entity.playerId !== Player.id) {
          this._font?.draw(
            name,
            entity.physicsObject.position.X + 48 - width / 2,
            entity.physicsObject.position.Y - 32,
            24,
            color
          );
        }
      }
    }

    /// Render the collision boxes.
    if (this.renderCollisionBoxes) {
      for (const entity of World.entities.values()) {
        this.primativeSprite.color = {
          r: 0.0,
          g: 1.0,
          b: 0.0,
          a: 0.25,
        };

        if (entity.collider !== null) {
          this.primativeSprite.draw(
            entity.collider.position.X,
            entity.collider.position.Y,
            entity.collider.size.X,
            entity.collider.size.Y
          );
        }
      }
    }

    // If the server world should not be rendered, return.
    if (!this.renderServerWorld) return;

    // For each entity in the server's world, render it.
    for (const entity of World.serverEntities.values()) {
      if (entity.physicsObject.type !== PhysicsObjectType.Dynamic) {
        continue;
      }
      this._sprite.color = {
        r: 1.0,
        g: 0.0,
        b: 0.0,
        a: 0.25,
      };
      this._sprite.draw(
        entity.physicsObject.position.X,
        entity.physicsObject.position.Y,
        96,
        96
      );
    }
  }

  /**
   * Get the sprite to draw primatives.
   * @returns The sprite.
   */
  public static get primativeSprite(): Sprite {
    if (!this._primativeSprite) {
      if (!this.standardShader) {
        throw new Error("Standard shader is not initialized.");
      }
      this._primativeSprite = new Sprite(
        this.standardShader,
        Texture.whiteTexture
      );
    }
    return this._primativeSprite;
  }
}
