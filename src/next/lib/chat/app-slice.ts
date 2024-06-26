import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { type Alert, type AppState, initialState } from "./app-state";
import { type Message } from "../types";

export const appSlice = createSlice({
  name: "app",
  initialState,
  reducers: {
    addAlert: (state: AppState, action: PayloadAction<Alert>) => {
      if (state.alerts.length === 3) {
        state.alerts.shift();
      }
      state.alerts.push(action.payload);
    },
    removeAlert: (state: AppState, action: PayloadAction<number>) => {
      state.alerts.splice(action.payload, 1);
    },
    addMessage: (
      state: AppState,
      action: PayloadAction<{ message: Message }>
    ) => {
      const message = action.payload.message;
      state.messages.push({ ...message });
    },
    abortMessage: (
      state: AppState,
      action: PayloadAction<{ message: Message }>
    ) => {
      const lastMessage = state.messages.length
        ? state.messages[state.messages.length - 1]
        : undefined;
      if (lastMessage?.id === action.payload.message.id) {
        state.messages.splice(-1);
        state.status = undefined;
      }
    },
    addMessages: (
      state: AppState,
      action: PayloadAction<{ messages: Message[]; status?: string }>
    ) => {
      const { messages, status } = action.payload;
      state.messages.push(...messages);
      state.status = status;
    },
    setStatus: (
      state: AppState,
      action: PayloadAction<{ status?: string }>
    ) => {
      state.status = action.payload.status;
    },
    restart: (state: AppState, action: PayloadAction<{ status?: string }>) => {
      state.status = action.payload.status;
    },
  },
});

export const {
  addAlert,
  removeAlert,
  addMessage,
  abortMessage,
  addMessages,
  setStatus,
  restart,
} = appSlice.actions;

export default appSlice.reducer;
