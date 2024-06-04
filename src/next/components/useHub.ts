"use client";

import * as React from "react";
import * as signalR from "@microsoft/signalr";
import useSWR from "swr";
import { generateId } from "ai";

export type MessageTextContent = {
  metadata: {
    Id: string;
  };
  text: string;
};

export type MessageContent = {
  role: {
    label: string;
  };
  items: MessageTextContent[];
};

export type Message = {
  id: string;
  role: string;
  content: string;
};

export type JSONValue =
  | null
  | string
  | number
  | boolean
  | { [x: string]: JSONValue }
  | Array<JSONValue>;

export const useHub = ({
  initialInput = "",
  initialMessages,
}: {
  initialInput?: string;
  initialMessages?: Message[];
}) => {
  const chatKey = "agent";

  const [initialMessagesFallback] = React.useState([]);

  // Store the chat state in SWR, using the chatId as the key to share states.
  const { data: messages, mutate } = useSWR<Message[]>(
    [chatKey, "messages"],
    null,
    { fallbackData: initialMessages ?? initialMessagesFallback }
  );
  // Keep the latest messages in a ref.
  const messagesRef = React.useRef<Message[]>(messages || []);
  React.useEffect(() => {
    messagesRef.current = messages || [];
  }, [messages]);

  // We store loading state in another hook to sync loading states across hook invocations
  const { data: isLoading = false, mutate: mutateLoading } = useSWR<boolean>(
    [chatKey, "loading"],
    null
  );

  const { data: error = undefined, mutate: setError } = useSWR<
    undefined | Error
  >([chatKey, "error"], null);

  const connectionRef = React.useRef<signalR.HubConnection | null>(null);
  React.useEffect(() => {
    const connect = async () => {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5000/agent")
        .build();
      await connection.start();
      connectionRef.current = connection;
    };

    if (connectionRef.current) {
      return;
    }

    connect();

    return () => {
      connectionRef.current?.stop();
      connectionRef.current == null;
    };
  }, [connectionRef]);

  const triggerRequest = React.useCallback(
    async ({ data, message }: { data: Message[]; message: Message }) => {
      try {
        if (connectionRef.current) {
          const result = connectionRef.current.stream<MessageContent>(
            "Streaming",
            message.content
          );

          // Do an optimistic update to the chat state to show the updated messages
          // immediately. Otherwise, users have to wait until message comes back to 
          // see the input
          const previousMessages = messagesRef.current;
          mutate(data, false);

          const subscription = result.subscribe({
            next: (value) => {
              const item = value.items.length ? value.items[0] : undefined;
              if (item) {
                mutate(
                  [
                    ...data,
                    {
                      id: item.metadata.Id,
                      role: value.role.label,
                      content: item.text,
                    },
                  ],
                  false
                );
              }
            },
            error: (err) => {
              // Restore the previous messages if the request fails.
              mutate(previousMessages, false);
              setError(err);
            },
            complete: () => {
              subscription.dispose();
            },
          });
        }
      } catch (err) {
        setError(err as Error);
      } finally {
        mutateLoading(false);
      }
    },
    [mutate, mutateLoading, setError, connectionRef, messagesRef]
  );

  const append = React.useCallback(
    async (input: string) => {
      const message = { id: generateId(), role: "user", content: input };
      return triggerRequest({
        data: messagesRef.current.concat(message as Message),
        message,
      });
    },
    [triggerRequest]
  );

  // Input state and handlers.
  const [input, setInput] = React.useState(initialInput);

  const handleSubmit = React.useCallback(
    (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault();
      if (!input) return;

      append(input);

      setInput("");
    },
    [input, append]
  );

  const handleInputChange = (e: any) => {
    setInput(e.target.value);
  };

  return {
    messages: messages || [],
    error,
    input,
    handleInputChange,
    handleSubmit,
    isLoading,
  };
};
