"use client";

import * as React from "react";
import * as signalR from "@microsoft/signalr";
import useSWR from "swr";
import { Message } from "../types";
import { nanoid } from "../utils";

export const useHub = ({
  initialMessages = [],
}: {
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
        mutateLoading(true);
        if (connectionRef.current) {
          const result = connectionRef.current.stream<Message>(
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
              mutate(
                [
                  ...data,
                  {
                    ...value,
                  },
                ],
                false
              );
            },
            error: (err) => {
              // Restore the previous messages if the request fails.
              mutate(previousMessages, false);
              setError(err);
            },
            complete: () => {
              subscription.dispose();
              mutateLoading(false);
            },
          });
        }
      } catch (err) {
        setError(err as Error);
      }
    },
    [mutate, mutateLoading, setError, connectionRef, messagesRef]
  );

  const append = React.useCallback(
    async (input: string) => {
      const message = { id: nanoid(), role: "user", content: input };
      return triggerRequest({
        data: messagesRef.current.concat(message as Message),
        message,
      });
    },
    [triggerRequest]
  );

  const handleSubmit = React.useCallback(
    async (value: string) => {
      await append(value);
    },
    [append]
  );

  return {
    messages: messages || [],
    error,
    handleSubmit,
    append,
    isLoading,
  };
};
