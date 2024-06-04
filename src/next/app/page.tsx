"use client";

import { MessageContent, useHub } from "@/components/useHub";

// import { Message, useChat } from 'ai/react';

export default function Chat() {
  /*
  const { messages, input, handleInputChange, handleSubmit, error } = useChat({
    api: '/api/use-chat-streamdata',
  });
  */

  const { messages, input, handleInputChange, handleSubmit } = useHub({
    initialMessages: [],
  });

  return (
    <div className="flex flex-col w-full max-w-screen-md py-24 mx-auto stretch">
      {messages?.map((x: MessageContent) => (
        <div key={x.id} className="whitespace-pre-wrap">
          <strong>{`${x.role}: `}</strong>
          {x.content}
          <br />
          <br />
        </div>
      ))}

      <form onSubmit={handleSubmit}>
        <input
          className="fixed bottom-0 w-full max-w-screen-md p-2 mb-8 border border-gray-300 rounded shadow-xl"
          value={input}
          placeholder="Say something..."
          onChange={handleInputChange}
        />
      </form>
    </div>
  );
}
