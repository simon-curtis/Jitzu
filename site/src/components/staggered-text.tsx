import { motion } from "framer-motion";

interface StaggeredTextProps {
  text: string;
  className?: string;
  once?: boolean;
}

const containerVariants = {
  hidden: {},
  visible: {
    transition: {
      staggerChildren: 0.04,
    },
  },
};

const wordVariants = {
  hidden: { y: 20, opacity: 0 },
  visible: {
    y: 0,
    opacity: 1,
    transition: { duration: 0.4, ease: "easeOut" as const },
  },
};

export function StaggeredText({
  text,
  className,
  once = true,
}: StaggeredTextProps) {
  const words = text.split(" ");

  return (
    <motion.span
      variants={containerVariants}
      initial="hidden"
      whileInView="visible"
      viewport={{ once, amount: 0.2 }}
      className={className}
      style={{ display: "inline-flex", flexWrap: "wrap", gap: "0.3em" }}
    >
      {words.map((word, i) => (
        <motion.span key={i} variants={wordVariants} style={{ display: "inline-block" }}>
          {word}
        </motion.span>
      ))}
    </motion.span>
  );
}
