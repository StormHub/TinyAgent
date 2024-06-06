"use client";

import * as React from "react";
import { useScrollAnchor } from "@/lib/hooks/use-scroll-anchor";
import { ChatList } from "./chat-list";
import { ChatPanel } from "./chat-panel";
import { Message } from "@/lib/types";

export const Chat = ({
  messages,
  onSubmit,
}: {
  messages: Message[];
  onSubmit: (input: string) => Promise<void>;
}) => {
  const [input, setInput] = React.useState("");
  const { messagesRef, scrollRef, visibilityRef, isAtBottom, scrollToBottom } =
    useScrollAnchor();

  return (
    <div
      className="flex flex-col w-full max-w-screen-md py-24 mx-auto stretch"
      ref={scrollRef}
    >
      <div ref={messagesRef}>
        <ChatList messages={messages} />
      </div>
      <div className="w-full h-px" ref={visibilityRef} />
      <ChatPanel
        input={input}
        setInput={setInput}
        isAtBottom={isAtBottom}
        scrollToBottom={scrollToBottom}
        onSubmit={onSubmit}
      />
    </div>
  );
};
