"use client";

import * as React from "react";
import { Chat } from "@/components/chat";

export default function IndexPage() {
  return (
    <div className="relative flex h-[calc(100vh_-_theme(spacing.16))] overflow-hidden">
      <Chat />
    </div>
  );
}
