/**
 * Message interface
 * @type {string} type - The type of the message
 * @type {string} content - The content of the message
 */
export class Message {
  public type: string;
  public content: string;

  constructor(type: string, content: any) {
    this.type = type;
    this.content = JSON.stringify(content);
  }
}
