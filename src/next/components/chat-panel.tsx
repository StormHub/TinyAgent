import * as React from "react";
import { ButtonScrollToBottom } from "./button-scroll-to-bottom";
import { PromptForm } from "./prompt-form";
import { Message } from "@/lib/types";

export const ChatPanel = ({
  input,
  setInput,
  isAtBottom,
  scrollToBottom,
  messages,
  status,
}: {
  id?: string;
  input: string;
  setInput: (value: string) => void;
  isAtBottom: boolean;
  scrollToBottom: () => void;
  messages: Message[];
  status?: string;
}) => {
  return (
    <div className="fixed inset-x-0 bottom-0 w-full bg-gradient-to-b from-muted/30 from-0% to-muted/30 to-50% duration-300 ease-in-out animate-in dark:from-background/10 dark:from-10% dark:to-background/80 peer-[[data-state=open]]:group-[]:lg:pl-[250px] peer-[[data-state=open]]:group-[]:xl:pl-[300px]">
      <ButtonScrollToBottom
        isAtBottom={isAtBottom}
        scrollToBottom={scrollToBottom}
      />
      <div className="mx-auto mb-6 sm:max-w-2xl sm:px-2">
        <div className="space-y-4 border-t bg-gray-50 shadow-lg sm:rounded-xl sm:border md:py-2">
          <PromptForm
            input={input}
            setInput={setInput}
            messages={messages}
            status={status}
          />
        </div>
      </div>
    </div>
  );
};
