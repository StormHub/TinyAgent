import * as signalR from "@microsoft/signalr";
import { StoreMiddlewareAPI } from "../redux/store";
import { addAlert } from "./app-slice";

const connectionToHub = () => {
  const url = new URL(
    process.env.AGENT_HUB_URL || "http://localhost:5000/agent"
  );
  
  const options = {
    skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets,
    logger: signalR.LogLevel.Warning,
  };

  const hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(url.toString(), options)
    .withAutomaticReconnect()
    .withHubProtocol(new signalR.JsonHubProtocol())
    .configureLogging(signalR.LogLevel.Information)
    .build();

  hubConnection.serverTimeoutInMilliseconds = 60000;

  return hubConnection;
};

const registerConnectionEvents = (
  hubConnection: signalR.HubConnection,
  store: StoreMiddlewareAPI
) => {
  hubConnection.onclose((error) => {
    if (hubConnection.state === signalR.HubConnectionState.Disconnected) {
      const errorMessage =
        "Connection closed due to error. Try refreshing this page to restart the connection";
      store.dispatch(
        addAlert({
          message: String(errorMessage),
          type: "error",
        })
      );
      console.log(errorMessage, error);
    }
  });

  hubConnection.onreconnecting((error) => {
    if (hubConnection.state === signalR.HubConnectionState.Reconnecting) {
      const errorMessage = "Connection lost due to error. Reconnecting...";
      store.dispatch(
        addAlert({
          message: String(errorMessage),
          type: "info",
        })
      );
      console.log(errorMessage, error);
    }
  });

  hubConnection.onreconnected((connectionId = "") => {
    if (hubConnection.state === signalR.HubConnectionState.Connected) {
      const message =
        "Connection reestablished. Please refresh the page to ensure you have the latest data.";
      store.dispatch(addAlert({ message, type: "success" }));
      console.log(`${message} Connected with connectionId ${connectionId}`);
    }
  });
};

const startConnection = (
  hubConnection: signalR.HubConnection,
  store: StoreMiddlewareAPI
) => {
  registerConnectionEvents(hubConnection, store);
  hubConnection
    .start()
    .then(() => {
      console.assert(
        hubConnection.state === signalR.HubConnectionState.Connected
      );
      console.log("SignalR connection established");
    })
    .catch((err) => {
      console.assert(
        hubConnection.state === signalR.HubConnectionState.Disconnected
      );
      console.error("SignalR Connection Error: ", err);
      setTimeout(() => {
        startConnection(hubConnection, store);
      }, 5000);
    });
};

// This is a singleton instance of the SignalR connection
let hubConnection: signalR.HubConnection | undefined = undefined;

export const getHubConnection = (store: StoreMiddlewareAPI) => {
  if (hubConnection === undefined) {
    hubConnection = connectionToHub();
    startConnection(hubConnection, store);
  }
  return hubConnection;
};
