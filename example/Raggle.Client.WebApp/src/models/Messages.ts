export interface TextContent {
  type: "text";
  index?: number;
  text?: string;
}

export interface ImageContent {
  type: "image";
  index?: number;
  data?: string;
}

export interface ToolContent {
  type: "tool";
  index?: number;
  id?: string;
  name?: string;
  arguments?: string;
  result?: object;
}

export type MessageContent = TextContent | ImageContent | ToolContent;

export type MessageRole = "user" | "assistant";

export interface Message {
  role?: MessageRole;
  name?: string;
  content?: MessageContent[];
  timeStamp?: Date;
}

export interface Conversation {
  id?: string;
  messages?: Message[];
  createdAt?: Date;
}
