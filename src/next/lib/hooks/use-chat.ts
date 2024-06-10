import { addMessage } from "../chat/app-slice";
import { useAppDispatch } from "../redux/hooks";
import { nanoid } from "../utils";

export const useChat = () => {
  const dispatch = useAppDispatch();

  const sendMessage = (content: string) => {
    dispatch(addMessage({ message: { id: nanoid(), role: "user", content } }));
  };

  return {
    sendMessage,
  };
};
