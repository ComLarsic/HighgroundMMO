import { SocketConnection } from "$lib/networking/socket_connection";

/// The player input state to send to the server.
export interface PlayerInput {
  x: number;
  y: number;
  jumping: boolean;
}

/**
 * The input manager class is responsible for managing input from the user.
 */
export class InputManager {
  /// The sate of the keys in the previous frame
  private static _previousKeys: Map<string, boolean> = new Map();
  /// The state of the keys in the current frame
  private static _keys: Map<string, boolean> = new Map();

  /// The previous player input state
  private static _previousPlayerInput: PlayerInput = {
    x: 0,
    y: 0,
    jumping: false,
  };

  /**
   * Initialize the input manager.
   */
  public static init() {
    window.addEventListener("keydown", (event) => {
      this._keys.set(event.key, true);
    });

    window.addEventListener("keyup", (event) => {
      this._keys.set(event.key, false);
    });
  }

  /**
   * Update the input manager.
   */
  public static update() {
    this._previousKeys = new Map(this._keys);
  }

  /**
   * Returns true if the key is currently pressed.
   * @param key The key to check.
   * @returns True if the key is currently pressed.
   */
  public static isKeyDown(key: string): boolean {
    return this._keys.get(key) || false;
  }

  /**
   * Returns true if the key was pressed this frame.
   * @param key The key to check.
   * @returns True if the key was pressed this frame.
   */
  public static isKeyPressed(key: string): boolean {
    const currentDown = this._keys.get(key) || false;
    const previousDown = this._previousKeys.get(key) || false;
    return currentDown && !previousDown;
  }

  /**
   * Returns true if the key was released this frame.
   * @param key The key to check.
   * @returns True if the key was released this frame.
   */
  public static isKeyReleased(key: string): boolean {
    const currentDown = this._keys.get(key) || false;
    const previousDown = this._previousKeys.get(key) || false;
    return !currentDown && previousDown;
  }

  /**
   * Get the player input state.
   * @returns The player input state.
   */
  public static getPlayerInput(): PlayerInput {
    return {
      x: (this.isKeyDown("d") ? 1 : 0) - (this.isKeyDown("a") ? 1 : 0),
      y: (this.isKeyDown("s") ? 1 : 0) - (this.isKeyDown("w") ? 1 : 0),
      jumping: this.isKeyDown(" "),
    };
  }

  /**
   * Send the player input state to the server.
   */
  public static updateServer() {
    if (!SocketConnection.isConnected()) {
      return;
    }
    const input = this.getPlayerInput();
    // Only send the input if it has changed.
    if (
      input.x === this._previousPlayerInput.x &&
      input.y === this._previousPlayerInput.y &&
      input.jumping === this._previousPlayerInput.jumping
    ) {
      return;
    }

    this._previousPlayerInput = input;
    SocketConnection.send("player-input", input);
  }
}
