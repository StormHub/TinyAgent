"use client";

import { getHubConnection } from "@/lib/chat/signalr-connection";
import { store } from "@/lib/redux/store";
import * as React from "react";
import { Provider } from "react-redux";

export function Providers({ children }: React.PropsWithChildren) {
  React.useEffect(() => {
    const connection = getHubConnection(store);
    console.log(connection.state);
  }, []);

  return <Provider store={store}>{children}</Provider>;
}
