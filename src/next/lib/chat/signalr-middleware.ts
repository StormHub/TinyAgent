import { Action, Dispatch, Middleware } from "@reduxjs/toolkit";
import { type Message } from "../types";
import { RootState, StoreMiddlewareAPI, resetState } from "../redux/store";
import { getHubConnection } from "./signalr-connection";
import { addAlert } from "./app-slice";

type SignalRAction = {
  payload: {
    message?: Message;
  };
} & Action;

export const signalRMiddleware: Middleware<
  any,
  RootState,
  Dispatch<SignalRAction>
> = (store: StoreMiddlewareAPI) => {
  return (next) => (action) => {
    const signalRAction = action as SignalRAction;
    const result = next(signalRAction);

    const hubConnection = getHubConnection(store);

    switch (signalRAction.type) {
      case "app/addMessage":
        {
          const message = signalRAction.payload.message;
          if (!message?.content) {
            return;
          }

          try {
            store.dispatch({
              type: "app/setStatus",
              payload: { status: "Thinking" },
            });

            const result = hubConnection.stream<Message>(
              "Streaming",
              message.content
            );

            const messages: Message[] = [];
            const subscription = result.subscribe({
              next: (value) => {
                messages.push(value);
              },
              error: (err) => {
                store.dispatch(
                  addAlert({ message: String(err), type: "Error" })
                );
                store.dispatch({
                  type: "app/abortMessage",
                  payload: { message },
                });
              },
              complete: () => {
                subscription.dispose();
                store.dispatch({
                  type: "app/addMessages",
                  payload: { messages, status: undefined },
                });
              },
            });
          } catch (err) {
            store.dispatch(addAlert({ message: String(err), type: "Error" }));
            store.dispatch({
              type: "app/abortMessage",
              payload: { message },
            });
          }
        }
        break;

      case "app/restart": {
        try {
          hubConnection
            .invoke("Restart")
            .catch((err) => {
              store.dispatch(addAlert({ message: String(err), type: "Error" }));
            })
            .finally(() => resetState());
        } catch (err) {
          store.dispatch(addAlert({ message: String(err), type: "Error" }));
        }
      }
    }

    return result;
  };
};
