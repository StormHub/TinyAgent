import { type Message } from "../types";

export type AppState = {
  alerts: Alert[];
  messages: Message[];
  status?: string;
};

export type Alert = {
  message: string;
  type: string;
  id?: string;
  onRetry?: () => void;
};

export const initialState: AppState = {
  alerts: [],
  messages: [],
};
