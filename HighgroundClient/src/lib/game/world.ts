import { Player } from "$lib/networking/player";
import { SocketConnection } from "$lib/networking/socket_connection";
import { lerp } from "math.gl";
import { min } from "mathjs";

/**
 * The type of a physics object.
 */
export enum PhysicsObjectType {
  Static = 0,
  Dynamic = 1,
}

/// Represents an entity in the game world.
export interface Entity {
  id: string;
  playerId: string | null;
  physicsObject: {
    id: string;
    colliderId: string;
    type: PhysicsObjectType;
    position: { X: number; Y: number };
    velocity: { X: number; Y: number };
  };
  playerData: {
    id: string;
    playerInput: {
      direction: { X: number; Y: number };
      jumping: boolean;
      airTime: number;
    };
    airTime: number;
  } | null;
  entityData: {
    id: string;
    position: { X: number; Y: number };
    velocity: { X: number; Y: number };
  } | null;
  collider: {
    id: string;
    position: { X: number; Y: number };
    size: { X: number; Y: number };
  } | null;
}

/// Represents a world-update message from the server.
export interface WorldUpdateMessage {
  deltaTime: number;
  addedEntities: Entity[];
  removedEntities: string[];
  updatedEntities: Entity[];
}

/**
 * The current state of the game world.
 * This class is responsible for managing the game world.
 * It should match the server's world state as closely as possible.
 */
export class World {
  /// The world update message backlog.
  private static _worldUpdateMessageBacklog: WorldUpdateMessage[] = [];

  /// A map of entities in the world.
  public static entities: Map<string, Entity> = new Map();

  /// The entities in the world as of the last server update.
  public static serverEntities: Map<string, Entity> = new Map();

  /// The delta time of the last world update.
  public static serverDeltaTime: number = 0;

  /// Update the world state.
  public static update(): void {
    this.predictWorldState();
    this.convergeWithServerState();
  }

  /**
   * Predicts the world state based on the state of the entities and the server's delta time.
   */
  private static predictWorldState(): void {
    for (const entity of this.entities.values()) {
      if (!entity.physicsObject) {
        continue;
      }

      entity.physicsObject.position.X +=
        entity.physicsObject.velocity.X * this.serverDeltaTime;
      entity.physicsObject.position.Y +=
        entity.physicsObject.velocity.Y * this.serverDeltaTime;

      if (entity.playerId === Player.id) {
        Player.physicsData = {
          position: { ...entity.physicsObject.position },
          velocity: { ...entity.physicsObject.velocity },
        };
      }
    }
  }

  /**
   * Process the world on backlogged messages.
   */
  public static processBackloggedMessages(): void {
    for (let i = 0; i < this._worldUpdateMessageBacklog.length; i++) {
      const message = this._worldUpdateMessageBacklog[i];
      this.applyWorldUpdateMessage(message);
      this._worldUpdateMessageBacklog.splice(i, 1);
    }
  }

  /**
   * Applies a world update message from the server to the world state.
   * @param message The world update message from the server.
   */
  private static applyWorldUpdateMessage(message: WorldUpdateMessage): void {
    this.serverDeltaTime = message.deltaTime;
    // Update existing entities.
    for (const entity of message.updatedEntities) {
      // Sometimes the server sends an update for an entity that was removed.
      if (message.removedEntities.includes(entity.id)) {
        continue;
      }
      this.serverEntities.set(entity.id, entity);
    }
    // Delete entities that no longer exist.
    for (const id of this.serverEntities.keys()) {
      if (message.removedEntities.find((removedId) => removedId === id)) {
        this.serverEntities.delete(id);
        this.entities.delete(id);
      }
    }

    // Add new entities to the world.
    for (const entity of message.addedEntities) {
      this.serverEntities.set(entity.id, entity);
      this.entities.set(entity.id, entity);
      console.log("Added entity:", entity);
    }

    this.serverDeltaTime = message.deltaTime;
    if (message.deltaTime > 0.25) {
      console.log("Large server tick delta: " + message.deltaTime);
    }
  }

  /**
   * Converge the client's world state with the server's world state.
   * This should be called after the client has processed all of the server's world update messages.
   */
  public static convergeWithServerState(): void {
    // Interpolate the client's world state with the server's world state.
    for (const entity of this.serverEntities.values()) {
      const clientEntity = this.entities.get(entity.id);
      // If the entity does not exist on the client, add it.
      if (!clientEntity) {
        this.entities.set(entity.id, { ...entity });
        continue;
      }

      // If the entity is the local player, do not interpolate.
      if (entity.playerId === Player.id) {
        clientEntity.physicsObject.velocity = {
          ...entity.physicsObject.velocity,
        };
      }

      const interpolationPower = 0.1;
      const lerpFactor = Math.pow(this.serverDeltaTime, interpolationPower);
      clientEntity.physicsObject.position.X = lerp(
        clientEntity.physicsObject.position.X,
        entity.physicsObject.position.X,
        lerpFactor
      );
      clientEntity.physicsObject.position.Y = lerp(
        clientEntity.physicsObject.position.Y,
        entity.physicsObject.position.Y,
        lerpFactor
      );
      clientEntity.physicsObject.velocity = {
        ...entity.physicsObject.velocity,
      };

      // Update collision data.
      if (entity.collider) {
        clientEntity.collider = { ...entity.collider };
      }
    }
  }

  /// Updates the world state based on a message from the server.
  public static recieveServerUpdate(message: WorldUpdateMessage): void {
    this._worldUpdateMessageBacklog.push(message);
  }
}

/// Listen for world-update messages from the server.
SocketConnection.on("world-update", async (message: WorldUpdateMessage) => {
  World.recieveServerUpdate(message);
});

SocketConnection.onConnect(async () => {
  // Clear the world state when the connection is established.
  World.entities.clear();
  World.serverEntities.clear();
  World.serverDeltaTime = 0;
});

// Clear the world state when the connection is closed.
SocketConnection.onClose(async () => {
  World.entities.clear();
  World.serverEntities.clear();
  World.serverDeltaTime = 0;
});
