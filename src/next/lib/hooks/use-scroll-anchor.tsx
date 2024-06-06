import * as React from "react";

export const useScrollAnchor = () => {
  const messagesRef = React.useRef<HTMLDivElement>(null);
  const scrollRef = React.useRef<HTMLDivElement>(null);
  const visibilityRef = React.useRef<HTMLDivElement>(null);

  const [isAtBottom, setIsAtBottom] = React.useState(true);
  const [isVisible, setIsVisible] = React.useState(false);

  const scrollToBottom = React.useCallback(() => {
    if (messagesRef.current) {
      messagesRef.current.scrollIntoView({
        block: "end",
        behavior: "smooth",
      });
    }
  }, []);

  React.useEffect(() => {
    if (messagesRef.current) {
      if (isAtBottom && !isVisible) {
        messagesRef.current.scrollIntoView({
          block: "end",
        });
      }
    }
  }, [isAtBottom, isVisible]);

  React.useEffect(() => {
    const { current } = scrollRef;

    if (current) {
      const handleScroll = (event: Event) => {
        const target = event.target as HTMLDivElement;
        const offset = 25;
        const isAtBottom =
          target.scrollTop + target.clientHeight >=
          target.scrollHeight - offset;

        setIsAtBottom(isAtBottom);
      };

      current.addEventListener("scroll", handleScroll, {
        passive: true,
      });

      return () => {
        current.removeEventListener("scroll", handleScroll);
      };
    }
  }, []);

  React.useEffect(() => {
    if (visibilityRef.current) {
      let observer = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting) {
              setIsVisible(true);
            } else {
              setIsVisible(false);
            }
          });
        },
        {
          rootMargin: "0px 0px -150px 0px",
        }
      );

      observer.observe(visibilityRef.current);

      return () => {
        observer.disconnect();
      };
    }
  });

  return {
    messagesRef,
    scrollRef,
    visibilityRef,
    scrollToBottom,
    isAtBottom,
    isVisible,
  };
};
