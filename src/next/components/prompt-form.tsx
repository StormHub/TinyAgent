"use client";

import * as React from "react";
import { useEnterSubmit } from "@/lib/hooks/use-enter-submit";
import Textarea from "react-textarea-autosize";
import { useChat } from "@/lib/hooks/use-chat";
import { IconNew } from "./ui/icons";
import { Button } from "./ui/button";

export const PromptForm = ({
  input,
  setInput,
}: {
  input: string;
  setInput: (value: string) => void;
}) => {
  const { sendMessage, restartChat } = useChat();

  const { formRef, onKeyDown } = useEnterSubmit();
  const inputRef = React.useRef<HTMLTextAreaElement>(null);
  React.useEffect(() => {
    if (inputRef.current) {
      inputRef.current.focus();
    }
  }, []);

  return (
    <form
      ref={formRef}
      onSubmit={(e: any) => {
        e.preventDefault();

        // Blur focus on mobile
        if (window.innerWidth < 600) {
          e.target["message"]?.blur();
        }

        const value = input.trim();
        setInput("");
        if (!value) return;

        sendMessage(value);
      }}
    >
      <div className="relative flex max-h-60 w-full grow flex-col bg-transparent px-1 sm:rounded-md sm:border sm:px-4 border-none">
        <Button
          className="outline rounded-full inline-flex items-center justify-center absolute ms-[-64px] z-10 left-0 top-[12px] size-8 rounded-full bg-gray-100 text-gray-400 p-0 sm:left-4"
          onClick={() => restartChat()}
        >
          <IconNew />
          <span className="sr-only">New Chat</span>
        </Button>

        <Textarea
          ref={inputRef}
          tabIndex={0}
          onKeyDown={onKeyDown}
          placeholder="Send a message."
          className="min-h-[60px] w-full resize-none bg-transparent px-1 py-[1rem] focus-within:outline-none sm:text-sm border-none"
          autoFocus
          spellCheck={false}
          autoComplete="off"
          autoCorrect="off"
          name="message"
          rows={1}
          value={input}
          onChange={(e) => setInput(e.target.value)}
        />
      </div>
    </form>
  );
};
