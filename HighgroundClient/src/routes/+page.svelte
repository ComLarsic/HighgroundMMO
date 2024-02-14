<script lang="ts">
  import { SocketConnection } from "$lib/networking/socket_connection";
  import { onMount } from "svelte";
  import Chat from "../components/Chat.svelte";
  import GameCanvas from "../components/GameCanvas.svelte";
  import { Player } from "$lib/networking/player";

  interface Room {
    id: string;
    name: string;
    capacity: number;
    players: Array<string>;
  }

  // The list of available rooms
  let rooms: Array<Room> = [];

  /// The connection form
  let connectionForm: {
    username: string;
    host: string;
  } = {
    username: "",
    host: "",
  };

  /// Keeps track of the connection status
  let isloaded = false;
  let isConnected = false;
  let isJoining = false;
  let isJoined = false;

  /// Keep track of the game canvas
  let gameCanvas: GameCanvas;

  /// Connect to the server
  async function connect(): Promise<void> {
    isJoining = true;
    localStorage.setItem("connectionForm", JSON.stringify(connectionForm));
    await SocketConnection.connect(connectionForm.host)
      .then(async () => {
        isConnected = true;
        // Update the room list
        await SocketConnection.send("get-room-list", {});
      })
      .finally(() => {
        isJoining = false;
      });
  }

  /// Disconnect from the server
  async function disconnect(): Promise<void> {
    isConnected = false;
    isJoined = false;
    rooms = [];
    await SocketConnection.close();
  }

  /// Join a room
  async function joinRoom(room: Room): Promise<void> {
    if (isConnected) {
      await SocketConnection.send("join", {
        username: connectionForm.username,
        room: room.id,
      });
    }
  }

  /// Handle the join event
  async function onJoin(message: any) {
    isJoined = message.success;
  }

  /// Handle the disconnect event
  function onDisconnect() {
    isConnected = false;
    isJoined = false;
    rooms = [];
  }

  // Add the disconnect event listener
  SocketConnection.onClose(onDisconnect);

  // Add the event listeners
  SocketConnection.on("join-response", onJoin);
  SocketConnection.on(
    "get-room-list-response",
    async (message: { rooms: Array<Room> }) => {
      rooms = message.rooms;
    }
  );

  onMount(() => {
    const storedForm = localStorage.getItem("connectionForm");
    if (storedForm) {
      connectionForm = JSON.parse(storedForm);
    }

    if (SocketConnection.isConnected()) {
      isConnected = true;
      if (Player.inRoom) {
        isJoined = true;
      } else {
        SocketConnection.send("get-room-list", {});
      }
    }

    isloaded = true;
  });
</script>

{#if isloaded}
  {#if isJoined}
    <div class="game-view">
      <div class="canvas-view">
        <GameCanvas bind:this={gameCanvas} />
      </div>
      <Chat />
    </div>
  {:else}
    <div class="menu">
      <h1>Highground Client</h1>
      <form>
        <div class="form-item">
          <label for="host">Host</label>
          <input id="host" bind:value={connectionForm.host} />
          {#if isConnected}
            <button
              disabled={isJoining}
              on:click={async () => await disconnect()}>Disconnect</button
            >
          {:else}
            <button disabled={isJoining} on:click={async () => await connect()}
              >Connect</button
            >
          {/if}
          <div
            id="indicator"
            class={isConnected ? "connected" : "disconnected"}
          ></div>
        </div>
        <div class="form-item">
          <label for="username">Username</label>
          <input id="username" bind:value={connectionForm.username} />
        </div>
      </form>
      <h2>Rooms</h2>
      <ul>
        {#each rooms as room}
          <li>
            <b>{room.name}:</b>{room.players.length}/{room.capacity}
            {#if room.players.length >= room.capacity}
              <b>Full</b>
            {:else}
              <button on:click={async () => await joinRoom(room)}>Join</button>
            {/if}
          </li>
        {/each}
      </ul>
    </div>
  {/if}
{/if}

<style lang="scss">
  .menu {
    display: flex;
    flex-direction: column;
    align-items: center;
    height: 100vh;

    form {
      display: flex;
      flex-direction: column;
      align-items: left;
      justify-content: center;
      gap: 1rem;
      width: fit-content;
    }

    .form-item {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 0.5rem;

      label {
        width: 5rem;
      }

      input {
        padding: 0.5rem;
        border-radius: 0.5rem;
        border: 1px solid #000;
      }

      button {
        padding: 0.5rem;
        border-radius: 0.5rem;
        border: 1px solid #000;
      }
    }

    ul {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    li {
      margin: 0.5rem 0;

      b {
        margin-right: 0.5rem;
      }

      button {
        margin-left: 0.5rem;
        align-self: right;
      }
    }

    #indicator {
      width: 1rem;
      height: 1rem;
      border-radius: 50%;
      margin-right: 0.5rem;

      &.connected {
        background-color: green;
      }

      &.disconnected {
        background-color: red;
      }
    }
  }
  .game-view {
    .canva-menu {
      position: absolute;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      top: 0;
      right: 0;
      z-index: 100;
    }
  }
</style>
