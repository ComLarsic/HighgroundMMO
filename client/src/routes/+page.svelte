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

  /// An error message to display
  let errorMessage: string | undefined = undefined;

  /// Check if the room is full
  function isRoomFull(room: Room): boolean {
    return room.players.length >= room.capacity;
  }

  /// Connect to the server
  async function connect(): Promise<void> {
    isJoining = true;
    errorMessage = undefined;
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
      await SocketConnection.send("join-room", {
        username: connectionForm.username,
        room: room.id,
      });
    }
  }

  /// Handle the join event
  async function onJoin(message: {}) {
    isJoined = true;
  }

  /// Handle the disconnect event
  function onDisconnect() {
    isConnected = false;
    isJoined = false;
    rooms = [];
    errorMessage = "Server disconnected";
  }

  /// Handle the error event
  function onError(message: any) {
    errorMessage = message.error;
    isJoining = false;
  }

  // Add the disconnect event listener
  SocketConnection.onClose(onDisconnect);
  // Add the error event listener
  SocketConnection.onError(onError);

  // Add the event listeners
  SocketConnection.on("join-room-response", onJoin);
  SocketConnection.on(
    "get-room-list-response",
    async (message: { rooms: Array<Room> }) => {
      console.log("Got room list", message.rooms);
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
  <div class="game-view">
    <div class="canvas-view">
      <GameCanvas bind:this={gameCanvas} />
    </div>

    {#if isJoined}
      <Chat />
    {:else}
      <div class="menu">
        <h1>Highground Client</h1>
        {#if errorMessage}
          <p class="error-message">{errorMessage}</p>
        {/if}

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
              <button
                disabled={isJoining}
                on:click={async () => await connect()}>Connect</button
              >
            {/if}
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
              <span class={"room-name " + isRoomFull(room) ? "full" : ""}>
                <span>{room.name} </span>
              </span>
              {#if isRoomFull(room)}
                <b>Full</b>
              {:else}
                <span class="room-players"
                  >{room.players.length}/{room.capacity}
                  <button on:click={async () => await joinRoom(room)}
                    >Join</button
                  >
                </span>
              {/if}
            </li>
          {/each}
        </ul>
      </div>
    {/if}
  </div>
{/if}

<style lang="scss">
  .menu {
    position: absolute;
    display: flex;
    flex-direction: column;
    align-items: center;

    // Display the menu in the center of the screen, pivoting from the center
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);

    width: 25%;

    color: white;
    font-family: "ADDSBP";

    z-index: 100;

    background-color: transparent;
    padding: 1rem;

    .error-message {
      color: red;
    }

    form {
      display: flex;
      flex-direction: column;
      align-items: left;
      justify-content: center;
      gap: 1rem;
      width: fit-content;

      h1 {
        margin: 0;
      }
    }

    .form-item {
      display: flex;
      flex-direction: row;
      align-items: center;
      gap: 0.5rem;

      label {
        color: white;
        left: 0;
      }
    }

    ul {
      list-style: none;
      padding: 0;
      margin: 0;
      width: 100%;
    }

    li {
      display: flex;
      flex-direction: row;
      margin: 0.5rem 0;
      align-items: center;

      .room-name {
        margin-right: 0.5rem;
        // Make the text left-aligned
        text-align: left;
        padding: 0.5rem;

        &.full {
          color: red;
        }
      }

      .room-players {
        margin-left: 0.5rem;
        // Align the button to the right
        margin-left: auto;
      }
    }

    input {
      padding: 0.5rem;
      border-radius: 0;
      color: #fff;
      font-family: "ADDSBP";
      border: 3px solid #fff;
      background-color: transparent;
      // Make the input fill the width of the form
      width: 100%;
    }

    button {
      padding: 0.5rem;
      border-radius: 0;
      border: 3px solid #fff;
      background-color: transparent;
      color: white;
      font-family: "ADDSBP";

      &:disabled {
        opacity: 0.5;
      }
    }
  }
</style>
