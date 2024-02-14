import { SocketConnection } from "./socket_connection";

/// The player data.
interface PlayerData {
  id: string;
  username: string;
  room: string;
}

/**
 * Represents a player in the game.
 */
export class Player {
  /// The player data
  private static _data: PlayerData | null = null;

  /**
   * Get the player's id.
   */
  private static getData(): PlayerData {
    if (this._data === null) {
      // Attempt to get the player data from local storage.
      const playerData = localStorage.getItem("player");
      if (playerData) {
        this._data = JSON.parse(playerData);
      }
    }
    return this._data ?? { id: "", username: "", room: "" };
  }

  /**
   * Get the player's id.
   * @returns The player's id.
   */
  public static get id(): string {
    return this.getData().id;
  }

  /**
   * Set the player's id.
   * @param id The player's id.
   */
  public static set id(id: string) {
    this._data = { ...this.getData(), id };
  }

  /**
   * Get the player's name.
   * @returns The player's name.
   */
  public static get name(): string {
    return this.getData().username;
  }

  /**
   * Set the player's name.
   * @param name The player's name.
   */
  public static set name(name: string) {
    this._data = { ...this.getData(), username: name };
  }

  /**
   * Get the player's room.
   * @returns The player's room.
   */
  public static get room(): string {
    return this.getData().room;
  }

  /**
   * Set the player's room.
   * @param room The player's room.
   */
  public static set room(room: string) {
    this._data = { ...this.getData(), room };
  }

  /**
   * Check if the player is in a room.
   * @returns True if the player is in a room, false otherwise.
   */
  public static get inRoom(): boolean {
    return this.room !== null;
  }
}

// Listen for join-response messages.
SocketConnection.on("join-response", async (message) => {
  Player.id = message.player.id;
  Player.name = message.player.username;
  Player.room = message.room;

  // Store the player data in local storage.
  localStorage.setItem(
    "player",
    JSON.stringify({
      id: Player.id,
      username: Player.name,
      room: Player.room,
    })
  );
});
