import { useRef, useMemo } from "react";
import { motion, useInView } from "framer-motion";

type Span = { text: string; color?: string; bold?: boolean; dim?: boolean };
type Line = Span[];

interface AnimatedTerminalProps {
  lines: Line[];
  staggerDelay?: number;
  className?: string;
  title?: string;
}

function RenderSpans({ spans }: { spans: Line }) {
  if (spans.length === 0) return <div className="h-3" />;
  return (
    <div className="whitespace-pre">
      {spans.map((span, i) => (
        <span
          key={i}
          style={{
            color: span.color,
            fontWeight: span.bold ? "bold" : undefined,
            opacity: span.dim ? 0.5 : undefined,
          }}
        >
          {span.text}
        </span>
      ))}
    </div>
  );
}

export function AnimatedTerminal({
  lines,
  staggerDelay = 0.08,
  className = "",
  title = "jitzu",
}: AnimatedTerminalProps) {
  const ref = useRef<HTMLDivElement>(null);
  const isInView = useInView(ref, { once: true, amount: 0.15 });

  const variants = useMemo(
    () => ({
      container: {
        hidden: {},
        visible: {
          transition: {
            staggerChildren: staggerDelay,
            delayChildren: 0.2,
          },
        },
      },
      line: {
        hidden: { opacity: 0, x: -8 },
        visible: {
          opacity: 1,
          x: 0,
          transition: { duration: 0.3, ease: "easeOut" as const },
        },
      },
    }),
    [staggerDelay]
  );

  return (
    <div ref={ref} className={`relative ${className}`}>
      <div className="relative rounded-2xl overflow-hidden border border-white/[0.06] bg-[#08080d]">
        {/* Terminal chrome */}
        <div className="flex items-center gap-2 px-5 py-3 bg-white/[0.02] border-b border-white/[0.06]">
          <div className="flex items-center gap-1.5">
            <div className="w-3 h-3 rounded-full bg-[#ff5f57]/80" />
            <div className="w-3 h-3 rounded-full bg-[#febc2e]/80" />
            <div className="w-3 h-3 rounded-full bg-[#28c840]/80" />
          </div>
          <span className="text-xs text-muted-foreground/60 ml-3 font-medium tracking-wide">
            {title}
          </span>
        </div>

        {/* Terminal content */}
        <motion.div
          variants={variants.container}
          initial="hidden"
          animate={isInView ? "visible" : "hidden"}
          className="p-6 overflow-auto"
          style={{
            fontFamily:
              'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
            fontSize: "0.8rem",
            lineHeight: "1.7",
            color: "#c8c8c8",
          }}
        >
          {lines.map((line, i) => (
            <motion.div key={i} variants={variants.line}>
              <RenderSpans spans={line} />
            </motion.div>
          ))}

          {/* Blinking cursor at the end */}
          <motion.div
            variants={variants.line}
            className="flex items-center gap-0"
          >
            <span style={{ color: "#5faf5f", fontWeight: "bold" }}>❯ </span>
            <motion.span
              animate={{ opacity: [1, 0, 1] }}
              transition={{ duration: 1.2, repeat: Infinity, ease: "linear", times: [0, 0.5, 1] }}
              style={{ color: "var(--pastel-blue)" }}
            >
              ▌
            </motion.span>
          </motion.div>
        </motion.div>
      </div>
    </div>
  );
}

// Pre-built compelling session data
const t = {
  user: "#5f8787",
  dir: "#87d7ff",
  arrow: "#5faf5f",
  branch: "#808080",
  dirty: "#d7af87",
  kw: "#d7afaf",
  str: "#afaf87",
  cmd: "#87af87",
  flag: "#87afaf",
  dim: "#808080",
  text: "#c8c8c8",
  match: "#d75f5f",
  size: "#87af87",
  dirColor: "#87afd7",
};

function prompt(dir: string, branch?: string): Line {
  const spans: Line = [
    { text: "simon", color: t.user },
    { text: " " },
    { text: dir, color: t.dir },
  ];
  if (branch) {
    spans.push({ text: ` (${branch})`, color: t.branch });
    spans.push({ text: " *", color: t.dirty });
  }
  return spans;
}

function cmd(spans: Span[]): Line {
  return [{ text: "❯ ", color: t.arrow, bold: true }, ...spans];
}

export const HERO_SESSION: Line[] = [
  prompt("~/projects/api", "main"),
  cmd([
    { text: "find", color: t.cmd },
    { text: " src " },
    { text: "-ext", color: t.flag },
    { text: " cs" },
  ]),
  [{ text: "src/Parser.cs", color: t.text }],
  [{ text: "src/Lexer.cs", color: t.text }],
  [{ text: "src/Interpreter.cs", color: t.text }],
  [],
  cmd([
    { text: "git log", color: t.text },
    { text: " --oneline", color: t.flag },
    { text: " | ", color: t.dim },
    { text: "first", color: t.cmd },
  ]),
  [{ text: "a1b2c3d Add pattern matching support", color: t.text }],
  [],
  cmd([
    { text: "let", color: t.kw },
    { text: " msg = " },
    { text: '"hello, world"', color: t.str },
  ]),
  cmd([{ text: "msg.to_upper()", color: t.text }]),
  [{ text: '"HELLO, WORLD"', color: t.str }],
  [],
  cmd([
    { text: "alias", color: t.cmd },
    { text: " ll=" },
    { text: '"ls -la"', color: t.str },
  ]),
  [{ text: "Alias set: ll → ls -la", color: t.dim }],
];
