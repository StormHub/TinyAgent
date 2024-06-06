import { Message } from "@/lib/types";
import { cn } from "@/lib/utils";
import * as React from "react";
import { IconOpenAI, IconUser } from "./ui/icons";
import { spinner } from "./ui/spinner";

const ChatMessage = ({ message }: { message: Message }) => {
  return (
    <div className={cn("group relative flex items-start md:-ml-12")}>
      <div
        className={cn(
          "flex size-6 shrink-0 select-none items-center justify-center rounded-md border shadow",
          message.role === "user"
            ? "bg-gray-500 text-gray-100"
            : "bg-gray-100 text-current"
        )}
      >
        {message.role === "user" ? <IconUser /> : <IconOpenAI />}
      </div>
      <div className="flex-1 px-2 ml-4 space-y-2 overflow-hidden">
        <p className="mb-2 last:mb-0">{message.content}</p>
      </div>
    </div>
  );
};

const SpinnerMessage = () => {
  return (
    <div className={cn("group relative flex items-start md:-ml-12")}>
      <div
        className={cn(
          "flex size-6 shrink-0 select-none items-center justify-center rounded-md border shadow bg-gray-100 text-current"
        )}
      >
        <IconOpenAI />
      </div>
      <div className="flex flex-row items-center flex-1 px-2 ml-4 space-y-2 overflow-hidden">
        {spinner}
      </div>
    </div>
  );
};

export const ChatList = ({
  messages,
  isLoading,
}: {
  messages: Message[];
  isLoading: boolean;
}) => {
  console.log(isLoading);
  
  return (
    <div className="relative mx-auto max-w-2xl px-4">
      {messages.map((message, index) => (
        <div key={message.id}>
          <ChatMessage message={message} />
          {index < messages.length - 1 && (
            <div className="shrink-0 bg-gray-200 h-[1px] w-full my-4" />
          )}
        </div>
      ))}
      {isLoading && (
        <>
          <div className="shrink-0 bg-gray-200 h-[1px] w-full my-4" />
          <SpinnerMessage />
        </>
      )}
    </div>
  );
};
