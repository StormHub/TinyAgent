"use client";

import * as React from "react";
import { Chat } from "@/components/chat";
import { useHub } from "@/lib/hooks/use-hub";

export default function IndexPage() {
  const { messages, handleSubmit, isLoading } = useHub({ initialMessages: [] });
  return (
    <div className="relative flex h-[calc(100vh_-_theme(spacing.16))] overflow-hidden">
      <Chat messages={messages} onSubmit={handleSubmit} isLoading={isLoading} />
    </div>
  );
}
