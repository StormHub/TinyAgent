import { Message } from "@/lib/types";
import { cn } from "@/lib/utils";
import * as React from "react";
import { IconOpenAI, IconUser } from "./ui/icons";

export const ChatList = ({ messages }: { messages: Message[] }) => {
  return (
    <div className="relative mx-auto max-w-2xl px-4">
      {messages.map((message, index) => (
        <div key={message.id}>
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
          {index < messages.length - 1 && (
            <div className="shrink-0 bg-gray-200 h-[1px] w-full my-4" />
          )}
        </div>
      ))}
    </div>
  );
};
