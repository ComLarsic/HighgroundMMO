import { Matrix4, lerp, mat4 } from "math.gl";
import { Gfx } from "./gfx";

/**
 * The camera class is used to control the view of the game world.
 * It should be used to control the view of the game world.
 */
export class Camera {
  // The camera position
  public static position: {
    X: number;
    Y: number;
  } = { X: 0, Y: 0 };

  /**
   * Make the camera focus on a point.
   * The point will be in the center of the screen.
   * @param point The point to focus on.
   */
  public static focusOn(point: { X: number; Y: number }, dt: number): void {
    const width = Gfx.width;
    const height = Gfx.height;

    // Calculate the target position
    const targetX = point.X - width / 2;
    const targetY = point.Y - height / 2;

    // Calculate the distance to the target
    const distance = Math.sqrt(
      Math.pow(targetX - this.position.X, 2) +
        Math.pow(targetY - this.position.Y, 2)
    );

    // Define damping and springiness factors
    const damping = 0.15;
    const springiness = 0.15 * distance > 0.0 || distance < 0.0 ? 1 : distance; // Scale springiness with distance

    // Calculate the velocity
    const velocityX = (targetX - this.position.X) * springiness;
    const velocityY = (targetY - this.position.Y) * springiness;

    // Update the position using smooth damping
    this.position.X += (velocityX - (this.position.X - targetX) * damping) * dt;
    this.position.Y += (velocityY - (this.position.Y - targetY) * damping) * dt;
  }

  /// Create the view matrix for the camera.
  public static getViewMatrix(): any[] | Float32Array {
    let lookat = mat4.create();
    mat4.lookAt(
      lookat,
      [this.position.X, this.position.Y, 1],
      [this.position.X, this.position.Y, 0],
      [0, 1, 0]
    );
    return lookat;
  }
}
