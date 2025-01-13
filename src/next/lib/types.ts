export type Message = {
  id: string;
  role: string;
  content: string;
};

export type AuthorRole = {
  label: string;
};

export type ContentMetadata = {
  Id: string;
};

export type ContentItem = {
  text: string;
}

export type ChatMessageContent = {
  items: ContentItem[];
  role: AuthorRole;
  metadata: ContentMetadata;
};
