"use client";

import * as React from "react";
import { useScrollAnchor } from "@/lib/hooks/use-scroll-anchor";
import { ChatList } from "./chat-list";
import { ChatPanel } from "./chat-panel";
import { RootState } from "@/lib/redux/store";
import { useAppSelector } from "@/lib/redux/hooks";

export const Chat = () => {
  const [input, setInput] = React.useState("");
  const { messages, status } = useAppSelector((state: RootState) => state.app);
  const { messagesRef, scrollRef, visibilityRef, isAtBottom, scrollToBottom } =
    useScrollAnchor();

  React.useEffect(() => {
    if (status && !isAtBottom) {
      scrollToBottom();
    }
  }, [status, isAtBottom, scrollToBottom]);

  return (
    <div
      className="group w-full overflow-auto pl-0 peer-[[data-state=open]]:lg:pl-[250px] peer-[[data-state=open]]:xl:pl-[300px]"
      ref={scrollRef}
    >
      <div className="pb-[48px] pt-4 md:pt-10" ref={messagesRef}>
        <ChatList messages={messages} status={status} />
      </div>
      <div className="w-full h-px" ref={visibilityRef} />
      <ChatPanel
        input={input}
        setInput={setInput}
        isAtBottom={isAtBottom}
        scrollToBottom={scrollToBottom}
      />
    </div>
  );
};
