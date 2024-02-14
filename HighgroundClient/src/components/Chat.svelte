<script lang="ts">
  import { Player } from "$lib/networking/player";
  import { SocketConnection } from "$lib/networking/socket_connection";
  import { onMount } from "svelte";

  /// The chat message type
  interface ChatMessage {
    player?: {
      id: string;
      username: string;
    };
    content: string;
  }

  // Keep track of the chat visibility
  let showChat = false;

  // Keep track of the chat messages
  let messages: ChatMessage[] = [];
  let messageInput: string = "";

  // The chat message box
  let messageBox: HTMLDivElement;

  /// Send a message to the server
  function sendMessage() {
    if (messageInput == "") return;

    // Send the message to the server
    SocketConnection.send("chat", {
      content: messageInput,
    });

    // Add the message to the chat log
    messages = [
      ...messages,
      {
        player: {
          id: Player.id,
          username: Player.name,
        },
        content: messageInput,
      },
    ];
    messageInput = "";
    scrollToBottom();
  }

  /// Handle the enter key being pressed
  function handleKeyPress(event: KeyboardEvent) {
    if (event.key === "Enter") {
      sendMessage();
    }
  }

  /// Toggle the chat visibility
  function toggleChat() {
    showChat = !showChat;
    if (showChat) {
      scrollToBottom();
    }
  }

  // Scroll to the bottom of the chat, but only if the user is already at the bottom
  function scrollToBottom() {
    if (messageBox === undefined) return;
    if (
      messageBox.scrollTop + messageBox.clientHeight ===
      messageBox.scrollHeight
    ) {
      setTimeout(() => {
        messageBox.scrollTop = messageBox.scrollHeight;
      }, 10);
    }
  }

  // Listen for chat messages
  SocketConnection.on("chat-sent", async (message: ChatMessage) => {
    // Don't add the message if it's from the current player (they already see it in the input box)
    if (message.player && message.player.id === Player.id) return;
    // Add the message to the chat log
    messages = [...messages, message];
    scrollToBottom();
  });

  // Listen for chat log response
  SocketConnection.on(
    "get-chat-response",
    async (log: { exception: string; messages: ChatMessage[] }) => {
      messages = log.messages;
      scrollToBottom();
    }
  );

  // Listen for players joining the room
  SocketConnection.on(
    "player-joined",
    async (player: { id: string; username: string }) => {
      // Add a message to the chat log
      messages = [
        ...messages,
        {
          player: undefined,
          content: player.username + " joined the room",
        },
      ];
      scrollToBottom();
    }
  );

  SocketConnection.on(
    "player-left",
    async (player: { id: string; username: string }) => {
      // Add a message to the chat log
      messages = [
        ...messages,
        {
          player: undefined,
          content: player.username + " left the room",
        },
      ];
      scrollToBottom();
    }
  );

  onMount(() => {
    // Request the chat log from the server
    SocketConnection.send("get-chat", {});
  });
</script>

<div class="chat-box">
  <button on:click={toggleChat}>Chat</button>
  {#if showChat}
    <div bind:this={messageBox} class="message-box">
      {#each messages as message}
        {#if message.player}
          <p class="player-message">
            <span class="player-name">[{message.player.username}]</span>:
            <span class="message-content">{message.content}</span>
          </p>
        {:else}
          <p class="server-message">
            <span class="server-name">[*]</span>:
            <span class="message-content">{message.content}</span>
          </p>
        {/if}
      {/each}
    </div>
    <div class="message-input">
      <input bind:value={messageInput} on:keypress={handleKeyPress} />
      <button on:click={sendMessage}>Send</button>
    </div>
  {/if}
</div>

<style lang="scss">
  .chat-box {
    position: absolute;
    display: flex;
    flex-direction: column;
    width: 256px;
    max-height: 30%;
    top: 0;
    right: 0;
    border: 1px solid white;

    // Display the chat box over the game canvas
    z-index: 100;

    .message-box {
      overflow-y: auto;
      color: white;
      height: 30%;
      padding: 0.5rem;

      p {
        margin: 0;

        &.player-message {
          .player-name {
            color: #00ff00;
          }
        }

        &.server-message {
          color: rgb(255, 248, 144);
        }
      }
    }

    .message-input {
      display: flex;

      input {
        background-color: transparent;
        color: white;
        flex: 1;
      }
    }

    button {
      padding: 0.5rem;
      border-radius: 0;
      border: 1px solid #fff;
      background-color: transparent;
      color: white;
    }
  }
</style>
