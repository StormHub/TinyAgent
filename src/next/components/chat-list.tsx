import { Message } from "@/lib/types";
import * as React from "react";
import { ChatMessage, WaitingIndicator } from "./chat-message";
import { nanoid } from "@/lib/utils";

const Divider = () => {
  return <div className="shrink-0 bg-gray-200 h-[1px] w-full my-4" />;
};

export const ChatList = ({
  messages,
  status,
}: {
  messages: Message[];
  status?: string;
}) => {
  return (
    <div className="relative mx-auto max-w-2xl px-4 pb-8">
      {messages.map((message, index) => (
        <div key={message.id}>
          <ChatMessage message={message} />
          {index < messages.length - 1 && <Divider />}
        </div>
      ))}
      {status && (
        <div key={nanoid()}>
          <Divider />
          <WaitingIndicator status={status} />
        </div>
      )}
    </div>
  );
};
