import { Message } from "@/lib/types";
import { cn } from "@/lib/utils";
import * as React from "react";
import { IconOpenAI, IconUser } from "./ui/icons";
import { spinner } from "./ui/spinner";
import remarkGfm from "remark-gfm";
import { MemoizedReactMarkdown } from "./ui/markdown";

const LinkAnchor = ({
  href,
  children,
}: React.AnchorHTMLAttributes<HTMLAnchorElement>) => {
  const handleOnClick = () => {
    if (window) {
      window.open(href, "_blank");
    }
  };
  return (
    <span
      className="text-blue-600 dark:text-blue-500 hover:underline cursor-pointer"
      onClick={handleOnClick}
    >
      {children}
    </span>
  );
};

export const ChatMessage = ({ message }: { message: Message }) => {
  return (
    <div className={cn("group relative flex items-start md:-ml-12")}>
      <div
        className={cn(
          "flex size-6 shrink-0 select-none items-center justify-center rounded-full border shadow",
          message.role === "user"
            ? "bg-gray-500 text-gray-100"
            : "bg-gray-100 text-current"
        )}
      >
        {message.role === "user" ? <IconUser /> : <IconOpenAI />}
      </div>
      <div className="flex-1 px-2 ml-4 space-y-2 overflow-hidden">
        <MemoizedReactMarkdown
          className="prose break-words dark:prose-invert prose-p:leading-relaxed prose-pre:p-0"
          remarkPlugins={[remarkGfm]}
          components={{
            a(props) {
              return <LinkAnchor {...props} />;
            },
            p({ children }) {
              return <p className="mb-2 last:mb-1">{children}</p>;
            },
            ul({ children }) {
              return <ul className="list-decimal">{children}</ul>;
            },
            ol({ children }) {
              return <ol className="list-decimal">{children}</ol>;
            },
            li({ children }) {
              return <li className="list-none mb-2 last:mb-1">{children}</li>;
            },
          }}
        >
          {message.content}
        </MemoizedReactMarkdown>
      </div>
    </div>
  );
};

export const WaitingIndicator = ({ status }: { status: string }) => {
  return (
    <div className={cn("group relative flex items-start md:-ml-12")}>
      <div
        className={cn(
          "flex size-6 shrink-0 select-none items-center justify-center rounded-full border shadow bg-gray-100 text-current"
        )}
      >
        <IconOpenAI />
      </div>
      <div className="flex flex-row items-center flex-1 px-2 ml-4 space-y-2 overflow-hidden">
        <div className="flex flex-column items-center">
          <div className="text-gray-400 me-1">{status}</div>
          <div>{spinner}</div>
        </div>
      </div>
    </div>
  );
};
