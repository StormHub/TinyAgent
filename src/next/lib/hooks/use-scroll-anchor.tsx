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
  }, [messagesRef]);

  React.useEffect(() => {
    const { current } = scrollRef;
    if (current) {
      const handleScroll = (event: Event) => {
        const target = event.target as HTMLDivElement;
        const offset = 32;
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
  }, [scrollRef]);

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
          rootMargin: "0px 0px -120px 0px",
        }
      );

      observer.observe(visibilityRef.current);

      return () => {
        observer.disconnect();
      };
    }
  }, [visibilityRef]);

  return {
    messagesRef,
    scrollRef,
    visibilityRef,
    scrollToBottom,
    isAtBottom,
    isVisible,
  };
};
