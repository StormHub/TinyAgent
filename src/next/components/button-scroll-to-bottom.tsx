"use client";

import * as React from "react";

import { cn } from "@/lib/utils";
import { IconArrowDown } from "./ui/icons";
import { Button } from "./ui/button";

export function ButtonScrollToBottom({
  className,
  isAtBottom,
  scrollToBottom,
  ...props
}: {
  isAtBottom: boolean;
  scrollToBottom: () => void;
} & React.ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <Button
      className={cn(
        "absolute right-4 top-1 z-10  bg-gray-50 transition-opacity duration-300 sm:right-8 md:top-2",
        isAtBottom ? "opacity-0" : "opacity-100",
        className
      )}
      onClick={() => scrollToBottom()}
      {...props}
    >
      <IconArrowDown />
      <span className="sr-only">Scroll to bottom</span>
    </Button>
  );
}
