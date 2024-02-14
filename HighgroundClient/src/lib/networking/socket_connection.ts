import { Message } from "./message";

/// A message handler function.
export type MessageHandler = (message: any) => Promise<void>;

/**
 * This class is responsible for creating a WebSocket connection to the server.
 * It also provides a method to send messages to the server.
 */
export class SocketConnection {
  private static _socket: WebSocket | null = null;

  /// A map of message handlers.
  private static _messageHandlers: Map<string, Array<MessageHandler>> =
    new Map();

  /// Callbacks for when the connection is established.
  private static _onConnectCallbacks: Array<() => void> = [];

  /// Callbacks for when the connection is closed.
  private static _onCloseCallbacks: Array<() => void> = [];

  /**
   * Get the socket connection.
   * @returns The WebSocket connection.
   */
  public static socket(): Promise<WebSocket> {
    const interval = 100;
    const maxAttempts = 10;
    return new Promise((resolve, reject) => {
      let attempts = 0;
      const intervalId = setInterval(() => {
        if (this._socket === null) {
          clearInterval(intervalId);
          reject("Attempted to get socket before it was created.");
        } else if (this._socket !== undefined) {
          clearInterval(intervalId);
          resolve(this._socket);
        } else if (attempts === maxAttempts) {
          clearInterval(intervalId);
          reject("No connection to the server could be established.");
        }
        attempts++;
      }, interval);
    });
  }

  /**
   * Sends a message to the server.
   * @param url The URL of the server to connect to.
   */
  public static async connect(url: string): Promise<void> {
    const max_time = 5_000;
    return new Promise((resolve, reject) => {
      // Set timeout
      setInterval(() => {
        reject("Connection attempt timed out");
      }, max_time);

      this._socket = new WebSocket("ws://" + url + "/ws");
      this._socket.onopen = (event) => {
        console.log("Connected to server.");
        resolve();
        this._onConnectCallbacks.forEach((callback) => callback());
      };
      this._socket.onerror = (event) => {
        console.error("Error connecting to server.");
      };
      this._socket.onclose = (event) => {
        console.log("Connection to server closed.");
        this._onCloseCallbacks.forEach((callback) => callback());
      };

      /// Handle incoming messages.
      this._socket.onmessage = (event) => {
        const message: Message = JSON.parse(event.data);
        const data: any = JSON.parse(message.content);
        const handlers = this._messageHandlers.get(message.type);
        if (handlers) {
          handlers.forEach((handler) => handler(data));
        }
      };
    });
  }

  /**
   * Sends a message to the server.
   * @param message The message to send.
   */
  public static async send(type: string, content: any): Promise<void> {
    const socket = await this.socket();
    socket.send(JSON.stringify(new Message(type, content)));
  }

  /**
   * Closes the connection to the server.
   */
  public static async close(): Promise<void> {
    const socket = await this.socket();
    socket.close();
    if (this._socket) {
      this._socket = null;
    }
  }

  /**
   * Returns the current state of the connection.
   * @returns The state of the connection.
   */
  public static isConnected(): boolean {
    if (this._socket === null) {
      return false;
    }
    return this._socket.readyState === WebSocket.OPEN;
  }

  /**
   * Adds a message handler.
   * @param type The type of message to handle.
   * @param handler The message handler.
   */
  public static on(type: string, handler: MessageHandler): void {
    if (this._messageHandlers.has(type)) {
      this._messageHandlers.get(type)?.push(handler);
    } else {
      this._messageHandlers.set(type, [handler]);
    }
  }

  /**
   * Adds a callback for when the connection is established.
   * @param callback The callback to add.
   */
  public static onConnect(callback: () => void): void {
    this._onConnectCallbacks.push(callback);
  }

  /**
   * Adds a callback for when the connection is closed.
   * @param callback The callback to add.
   */
  public static onClose(callback: () => void): void {
    this._onCloseCallbacks.push(callback);
  }
}
