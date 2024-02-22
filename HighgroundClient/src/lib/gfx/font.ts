import type { Shader } from "./shaders";
import { Sprite } from "./sprite";

/// The bitmap font renderer.
export class BitMapFont {
  /// The sprite to use for rendering fonts
  private _sprite: Sprite | null = null;

  // The width and height of a character.
  public charWidth = 8;
  public charHeight = 8;

  // The supported characters.
  private _charMap: string;

  public get sprite(): Sprite | null {
    return this._sprite;
  }

  public get charMap(): string {
    return this._charMap;
  }

  /**
   * Create a new bitmap font.
   * @param sprite The sprite to use for rendering the font.
   * @param charWidth The width of a character.
   * @param charHeight The height of a character.
   * @param charMap The characters supported by the font in the order they appear in the sprite.
   */
  constructor(
    sprite: Sprite,
    charWidth: number,
    charHeight: number,
    charMap: string
  ) {
    this._sprite = sprite;
    this.charWidth = charWidth;
    this.charHeight = charHeight;
    this._charMap = charMap;
  }

  /**
   * Load the font.
   * @param shader The shader to use for rendering the font.
   * @param font The font to load.
   */
  public static async load(
    shader: Shader,
    font: string,
    charWidth: number,
    charHeight: number,
    charMap: string
  ): Promise<BitMapFont> {
    const sprite = await Sprite.load(shader, font);
    return new BitMapFont(sprite, charWidth, charHeight, charMap);
  }

  /**
   * Render text to the screen using the font.
   * @param text The text to render.
   * @param x The x position to render the text.
   * @param y The y position to render the text.
   * @param size The size to render the text.
   */
  public draw(
    text: string,
    x: number,
    y: number,
    size: number,
    color: { r: number; g: number; b: number; a: number } = {
      r: 1,
      g: 1,
      b: 1,
      a: 1,
    }
  ) {
    if (!this._sprite) {
      throw new Error("Font is not initialized.");
    }

    // Set the sprite color.
    this._sprite.color = color;

    // Calculate the scale
    const scale = size / this.charWidth;
    let offsetX = 0;
    for (const char of text) {
      const charIndex = this._charMap.indexOf(char);
      if (charIndex === -1) {
        continue;
      }

      const charX = charIndex * this.charWidth;
      const charY = 0;

      this._sprite.drawBounded(
        {
          x: charX,
          y: charY,
          width: this.charWidth,
          height: this.charHeight,
        },
        {
          x: x + offsetX,
          y: y,
          width: size,
          height: size,
        }
      );

      offsetX += this.charWidth * scale;
    }
  }

  /// Based on the text, calculate the width of the text.
  public calculateWidth(text: string, size: number): number {
    return text.length * size;
  }
}
