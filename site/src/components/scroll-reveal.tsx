import { motion } from "framer-motion";
import type { CSSProperties, PropsWithChildren } from "react";

interface ScrollRevealProps extends PropsWithChildren {
  delay?: number;
  duration?: number;
  once?: boolean;
  className?: string;
  style?: CSSProperties;
}

export function ScrollReveal({
  children,
  delay = 0,
  duration = 0.6,
  once = true,
  className,
  style,
}: ScrollRevealProps) {
  return (
    <motion.div
      initial={{ y: 30, opacity: 0, filter: "blur(4px)" }}
      whileInView={{ y: 0, opacity: 1, filter: "blur(0px)" }}
      transition={{ duration, delay, ease: "easeOut" as const }}
      viewport={{ once, amount: 0.2 }}
      className={className}
      style={style}
    >
      {children}
    </motion.div>
  );
}
