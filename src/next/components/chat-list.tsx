import { Message } from "@/lib/types";
import * as React from "react";
import { ChatMessage, WaitingIndicator } from "./chat-message";
import { nanoid } from "@/lib/utils";

export const ChatList = ({
  messages,
  status,
}: {
  messages: Message[];
  status?: string;
}) => {
  return (
    <div className="relative mx-auto max-w-2xl px-4 pb-8 divide-y">
      {messages.map((message) => (
        <div className="py-2.5" key={message.id}>
          <ChatMessage message={message} />
        </div>
      ))}
      {status && (
        <div className="py-2.5" key={nanoid()}>
          <WaitingIndicator status={status} />
        </div>
      )}
    </div>
  );
};
