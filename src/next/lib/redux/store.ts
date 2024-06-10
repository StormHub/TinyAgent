import {
  Action,
  Dispatch,
  MiddlewareAPI,
  ThunkMiddleware,
  Tuple,
  UnknownAction,
  configureStore,
} from "@reduxjs/toolkit";
import { AppState } from "../chat/app-state";
import resetStateReducer, { resetApp } from "./root-reducer";
import { signalRMiddleware } from "../chat/signalr-middleware";

export type RootState = {
  app: AppState;
};

export type StoreMiddlewareAPI = MiddlewareAPI<Dispatch, RootState>;

export const store = configureStore<
  RootState,
  Action,
  Tuple<Array<ThunkMiddleware<RootState, UnknownAction>>>
>({
  reducer: resetStateReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(signalRMiddleware),
});

export type AppDispatch = typeof store.dispatch;

export const resetState = () => {
  store.dispatch(resetApp());
};
