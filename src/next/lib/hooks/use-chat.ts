import { addMessage, restart } from "../chat/app-slice";
import { useAppDispatch } from "../redux/hooks";
import { nanoid } from "../utils";

export const useChat = () => {
  const dispatch = useAppDispatch();

  const sendMessage = (content: string) => {
    dispatch(addMessage({ message: { id: nanoid(), role: "user", content } }));
  };

  const restartChat = () => {
    dispatch(restart({ status: "Starting new chat" }));
  };

  return {
    sendMessage,
    restartChat,
  };
};
