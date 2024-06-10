import {
  combineReducers,
  createAction,
  Reducer,
  UnknownAction,
} from "@reduxjs/toolkit";
import { type RootState } from "./store";
import appReducer from "../chat/app-slice";

export const resetApp = createAction("resetApp");

const rootReducer: Reducer<RootState> = combineReducers({
  app: appReducer,
});

export const resetAppReducer = (
  state: RootState | undefined,
  action: UnknownAction
) => {
  if (action.type === resetApp.type) {
    state = {
      app: appReducer(undefined, action),
    };
  }

  return rootReducer(state, action);
};

export default resetAppReducer;
