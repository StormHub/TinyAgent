"use client";

import * as React from "react";
import { useScrollAnchor } from "@/lib/hooks/use-scroll-anchor";
import { ChatList } from "./chat-list";
import { ChatPanel } from "./chat-panel";
import { RootState } from "@/lib/redux/store";
import { useAppDispatch, useAppSelector } from "@/lib/redux/hooks";
import { toast } from "sonner";
import { removeAlert } from "@/lib/chat/app-slice";

export const Chat = () => {
  const [input, setInput] = React.useState("");
  const dispatch = useAppDispatch();
  const { messages, status, alerts } = useAppSelector(
    (state: RootState) => state.app
  );
  const { messagesRef, scrollRef, visibilityRef, isAtBottom, scrollToBottom } =
    useScrollAnchor();

  React.useEffect(() => {
    if (status && !isAtBottom) {
      scrollToBottom();
    }
  }, [status, isAtBottom, scrollToBottom]);

  React.useEffect(() => {
    alerts.map((x, index) => {
      toast.error(`${x.message}`, {
        onAutoClose: () => dispatch(removeAlert(index))
      });
    });
  }, [alerts]);

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
