import { createOpenAI } from '@ai-sdk/openai';
import { StreamData, StreamingTextResponse, streamText } from 'ai';

// Allow streaming responses up to 30 seconds
export const maxDuration = 30;

const openai = createOpenAI({
  baseURL: 'http://localhost:11434/v1',
  apiKey: 'ollama',
});

export async function POST(req: Request) {
  const { messages } = await req.json();

  const result = await streamText({
    model: openai('phi3'),
    messages,
  });

  // optional: use stream data
  const data = new StreamData();

  data.append('initialized call');

  return new StreamingTextResponse(
    result.toAIStream({
      onFinal() {
        data.append('call completed');
        data.close();
      },
    }),
    {},
    data,
  );
}
