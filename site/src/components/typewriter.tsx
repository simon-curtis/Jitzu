import { useState, useEffect, useRef } from "react";
import { motion, useInView } from "framer-motion";

interface TypewriterProps {
  text: string;
  speed?: number;
  className?: string;
}

export function Typewriter({ text, speed = 25, className }: TypewriterProps) {
  const ref = useRef<HTMLDivElement>(null);
  const isInView = useInView(ref, { once: true, amount: 0.3 });
  const [displayed, setDisplayed] = useState("");
  const [done, setDone] = useState(false);
  const [cursorVisible, setCursorVisible] = useState(true);

  useEffect(() => {
    if (!isInView) return;

    let i = 0;
    const interval = setInterval(() => {
      if (i < text.length) {
        setDisplayed(text.slice(0, i + 1));
        i++;
      } else {
        clearInterval(interval);
        setDone(true);
      }
    }, speed);

    return () => clearInterval(interval);
  }, [isInView, text, speed]);

  useEffect(() => {
    if (!done) return;
    let blinks = 0;
    const interval = setInterval(() => {
      setCursorVisible((v) => !v);
      blinks++;
      if (blinks >= 6) {
        clearInterval(interval);
        setCursorVisible(false);
      }
    }, 400);
    return () => clearInterval(interval);
  }, [done]);

  return (
    <motion.div
      ref={ref}
      className={className}
      initial={{ opacity: 0 }}
      animate={isInView ? { opacity: 1 } : {}}
      transition={{ duration: 0.3 }}
    >
      <span>{displayed}</span>
      <span
        style={{
          opacity: cursorVisible ? 1 : 0,
          transition: "opacity 0.1s",
        }}
        className="text-pastel-blue"
      >
        |
      </span>
    </motion.div>
  );
}
