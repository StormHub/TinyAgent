"use client";

import * as React from "react";
import { useEnterSubmit } from "@/lib/hooks/use-enter-submit";
import Textarea from "react-textarea-autosize";

export const PromptForm = ({
  input,
  setInput,
  onSubmit,
}: {
  input: string;
  setInput: (value: string) => void;
  onSubmit: (input: string) => Promise<void>;
}) => {
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
      onSubmit={async (e: any) => {
        e.preventDefault();

        // Blur focus on mobile
        if (window.innerWidth < 600) {
          e.target["message"]?.blur();
        }

        const value = input.trim();
        setInput("");
        if (!value) return;

        await onSubmit(value);
      }}
    >
      <div className="relative flex max-h-60 w-full grow flex-col overflow-hidden bg-background px-4 sm:rounded-md sm:border sm:px-8">
        <Textarea
          ref={inputRef}
          tabIndex={0}
          onKeyDown={onKeyDown}
          placeholder="Send a message."
          className="min-h-[60px] w-full resize-none bg-transparent px-4 py-[1.3rem] focus-within:outline-none sm:text-sm"
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
