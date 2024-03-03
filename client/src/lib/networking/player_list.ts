import { Player, type PlayerData } from "./player";
import { SocketConnection } from "./socket_connection";

/**
 * A list of players in the game.
 */
export class PlayerList {
  private static _players: Map<string, PlayerData> = new Map();

  public static get players(): Map<string, PlayerData> {
    return this._players;
  }

  /**
   * Add a player to the list.
   * @param player The player to add.
   */
  public static add(player: PlayerData): void {
    this._players.set(player.id, player);
  }

  /**
   * Remove a player from the list.
   * @param player The player to remove.
   */
  public static remove(player: PlayerData): void {
    this._players.delete(player.id);
  }

  /**
   * Get a player by id.
   * @param id The id of the player to get.
   * @returns The player with the given id.
   */
  public static get(id: string): PlayerData | undefined {
    return this._players.get(id);
  }
}

SocketConnection.on(
  "player-joined",
  async (player: { id: string; username: string }) => {
    console.log("Player joined:", player);
    PlayerList.add({
      id: player.id,
      username: player.username,
      room: Player.room,
    });
  }
);

SocketConnection.on(
  "player-left",
  async (player: { id: string; username: string }) => {
    PlayerList.remove({
      id: player.id,
      username: player.username,
      room: Player.room,
    });
  }
);

SocketConnection.on(
  "join-response",
  async (response: {
    success: boolean;
    player: PlayerData;
    room: string;
    playerList: PlayerData[];
  }) => {
    if (!response.success) return;

    PlayerList.add({
      id: Player.id,
      username: Player.name,
      room: Player.room,
    });

    console.log("Player list:", response);

    for (const player of response.playerList) {
      PlayerList.add(player);
    }
  }
);
