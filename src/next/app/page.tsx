"use client";

import * as React from "react";
import { Chat } from "@/components/chat";
import { useHub } from "@/lib/hooks/use-hub";

export default function IndexPage() {
  const { messages, handleSubmit, isLoading } = useHub({ initialMessages: [] });
  return (
    <Chat messages={messages} onSubmit={handleSubmit} isLoading={isLoading} />
  );
}
