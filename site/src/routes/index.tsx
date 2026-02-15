import { Button } from "@/components/ui/button";
import { EnhancedParallaxBackground } from "@/components/enhanced-parallax-background";
import { ScrollReveal } from "@/components/scroll-reveal";
import { MagneticButton } from "@/components/magnetic-button";
import { AnimatedTerminal, HERO_SESSION } from "@/components/animated-terminal";
import { Marquee } from "@/components/marquee";
import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import { JitzuHighlighter } from "@/components/jitzu-highlighter";
import {
  GithubIcon,
  ChevronDownIcon,
  ArrowRightIcon,
  TerminalIcon,
  BookOpenIcon,
} from "lucide-react";
import { motion, useScroll, useTransform } from "framer-motion";
import { useRef } from "react";

export const Route = createFileRoute("/")({
  component: Index,
});

const BUILTINS = [
  "ls", "cd", "pwd", "cat", "grep", "find", "mkdir", "rm", "cp", "mv",
  "echo", "touch", "head", "tail", "sort", "uniq", "wc", "cut", "diff",
  "alias", "unalias", "label", "unlabel", "history", "clear", "exit",
  "env", "set", "export", "which", "whoami", "hostname", "uptime",
  "ps", "kill", "jobs", "fg", "bg", "sudo", "chmod", "open", "tee",
  "more", "less", "tree", "du", "df", "date", "time", "sleep", "seq",
  "yes", "true", "false", "test", "help", "version", "monitor",
];

function Index() {
  const navigate = useNavigate();
  const heroRef = useRef<HTMLDivElement>(null);
  const { scrollYProgress } = useScroll({
    target: heroRef,
    offset: ["start start", "end start"],
  });

  const heroScale = useTransform(scrollYProgress, [0, 1], [1, 0.6]);
  const heroOpacity = useTransform(scrollYProgress, [0, 0.8], [1, 0]);
  const heroY = useTransform(scrollYProgress, [0, 1], [0, -100]);

  return (
    <div className="flex flex-col min-h-screen bg-background">
      {/* HEADER */}
      <header className="fixed top-0 left-0 right-0 z-50 px-4 lg:px-6 bg-background/80 backdrop-blur-md border-b border-border/50">
        <div className="container mx-auto flex items-center h-16">
          <Link to="/" className="flex items-center">
            <span className="text-lg font-black text-gradient-primary">
              Jitzu
            </span>
          </Link>
          <nav className="ml-auto flex gap-6 items-center">
            <Link
              to="/docs"
              className="text-sm text-muted-foreground/70 hover:text-foreground transition-colors"
            >
              Docs
            </Link>
            <a
              href="#shell"
              className="text-sm text-muted-foreground/70 hover:text-foreground transition-colors"
            >
              Shell
            </a>
            <a
              href="#language"
              className="text-sm text-muted-foreground/70 hover:text-foreground transition-colors"
            >
              Language
            </a>
            <a
              href="https://github.com/simon-curtis/jitzu"
              className="text-muted-foreground/70 hover:text-foreground transition-colors"
            >
              <GithubIcon className="size-4" />
            </a>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        {/* ═══════════════════════════════════════════════════════
            HERO
            ═══════════════════════════════════════════════════════ */}
        <section
          ref={heroRef}
          className="relative h-[110vh] flex items-center justify-center overflow-hidden"
        >
          <EnhancedParallaxBackground
            enableParticles={true}
            particleCount={120}
          />

          <div
            className="absolute inset-0 pointer-events-none"
            style={{
              background:
                "radial-gradient(ellipse 80% 50% at 50% 40%, color-mix(in srgb, var(--pastel-blue) 8%, transparent), transparent)",
            }}
          />

          <motion.div
            style={{ scale: heroScale, opacity: heroOpacity, y: heroY }}
            className="relative z-10 text-center px-4"
          >
            <motion.h1
              initial={{ opacity: 0, y: 40 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 1.2, ease: [0.25, 0.1, 0, 1] }}
              className="text-[18vw] md:text-[15vw] lg:text-[12vw] font-black tracking-[-0.06em] leading-none text-gradient-animated select-none py-2"
            >
              Jitzu
            </motion.h1>

            <motion.p
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 1, delay: 0.8 }}
              className="text-lg md:text-2xl text-muted-foreground/70 mt-6 font-light tracking-wide"
            >
              An interactive shell and a typed scripting language.
            </motion.p>

            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 1, delay: 1.2 }}
              className="flex items-center justify-center gap-6 md:gap-10 mt-8"
            >
              {[
                { label: "Runtime", value: ".NET 10" },
                { label: "Packages", value: "Any NuGet library" },
                { label: "Platforms", value: "Win · Mac · Linux" },
                { label: "License", value: "Open source" },
              ].map((item, i) => (
                <div key={i} className="flex flex-col items-center">
                  <span className="text-[9px] uppercase tracking-[0.3em] text-muted-foreground/25 mb-0.5">
                    {item.label}
                  </span>
                  <span className="text-xs md:text-sm font-medium text-muted-foreground/50">
                    {item.value}
                  </span>
                </div>
              ))}
            </motion.div>

            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 1, delay: 1.6 }}
              className="mt-10 flex gap-4 justify-center"
            >
              <MagneticButton>
                <Button
                  size="lg"
                  variant="ghost"
                  onClick={() =>
                    navigate({ to: "/docs/getting-started/installation" })
                  }
                  className="text-muted-foreground/70 hover:text-foreground gap-2"
                >
                  <TerminalIcon className="size-4" />
                  Install
                </Button>
              </MagneticButton>
              <MagneticButton>
                <Button
                  size="lg"
                  variant="ghost"
                  onClick={() => navigate({ to: "/docs" })}
                  className="text-muted-foreground/70 hover:text-foreground gap-2"
                >
                  <BookOpenIcon className="size-4" />
                  Docs
                </Button>
              </MagneticButton>
              <MagneticButton>
                <a href="https://github.com/simon-curtis/jitzu">
                  <Button
                    size="lg"
                    variant="ghost"
                    className="text-muted-foreground/70 hover:text-foreground gap-2"
                  >
                    <GithubIcon className="size-4" />
                    GitHub
                  </Button>
                </a>
              </MagneticButton>
            </motion.div>
          </motion.div>

          <motion.div
            className="absolute bottom-12 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 2, duration: 1 }}
          >
            <span className="text-[10px] uppercase tracking-[0.3em] text-muted-foreground/30">
              Scroll
            </span>
            <motion.div
              animate={{ y: [0, 6, 0] }}
              transition={{
                duration: 2,
                repeat: Infinity,
                ease: "easeInOut",
              }}
            >
              <ChevronDownIcon className="size-4 text-muted-foreground/20" />
            </motion.div>
          </motion.div>
        </section>

        {/* ═══════════════════════════════════════════════════════
            COMPARISONS — Lead with the pain
            ═══════════════════════════════════════════════════════ */}
        <section className="relative py-24 overflow-hidden border-y border-white/[0.03]">
          <div className="container mx-auto px-4 md:px-6 relative z-10">
            <ScrollReveal>
              <div className="max-w-3xl mx-auto text-center mb-16">
                <h2 className="text-3xl md:text-5xl font-black tracking-[-0.04em] mb-4">
                  Same task. Less noise.
                </h2>
              </div>
            </ScrollReveal>

            <div className="max-w-3xl mx-auto space-y-3">
              {[
                {
                  them: "Get-ChildItem -Recurse -Filter *.cs",
                  us: "find -ext cs",
                  lang: "PowerShell",
                },
                {
                  them: "$obj | Select-Object -ExpandProperty Name",
                  us: "obj.name",
                  lang: "PowerShell",
                },
                {
                  them: "try { } catch [System.Exception] { $_ }",
                  us: "let val = risky()?",
                  lang: "PowerShell",
                },
                {
                  them: "result=$(cmd) && echo $result",
                  us: "let result = cmd(); print(result)",
                  lang: "Bash",
                },
                {
                  them: "everything is a string",
                  us: "Int, String, Bool, Result<T, E>, Option<T>",
                  lang: "Bash",
                },
              ].map((row, i) => (
                <ScrollReveal key={i} delay={i * 0.05}>
                  <div className="grid md:grid-cols-2 rounded-xl overflow-hidden border border-white/[0.04] text-sm font-mono">
                    <div className="px-5 py-4 bg-white/[0.01] border-b md:border-b-0 md:border-r border-white/[0.04]">
                      <span className="text-[10px] uppercase tracking-[0.2em] text-muted-foreground/30 block mb-2">
                        {row.lang}
                      </span>
                      <code className="text-muted-foreground/50">
                        {row.them}
                      </code>
                    </div>
                    <div className="px-5 py-4">
                      <span className="text-[10px] uppercase tracking-[0.2em] text-[var(--pastel-blue)]/40 block mb-2">
                        Jitzu
                      </span>
                      <code className="text-foreground/90">{row.us}</code>
                    </div>
                  </div>
                </ScrollReveal>
              ))}
            </div>
          </div>
        </section>

        {/* ═══════════════════════════════════════════════════════
            MARQUEE
            ═══════════════════════════════════════════════════════ */}
        <section className="relative py-20 overflow-hidden">
          <ScrollReveal>
            <p className="text-center text-sm text-muted-foreground/40 uppercase tracking-[0.25em] mb-8">
              60+ built-in commands
            </p>
          </ScrollReveal>

          <Marquee speed={60} className="opacity-[0.15]">
            <div className="flex gap-8">
              {BUILTINS.map((cmd) => (
                <span
                  key={cmd}
                  className="text-2xl md:text-4xl font-mono font-bold tracking-tight whitespace-nowrap text-foreground"
                >
                  {cmd}
                </span>
              ))}
            </div>
          </Marquee>

          <Marquee speed={45} reverse className="mt-4 opacity-[0.08]">
            <div className="flex gap-8">
              {[...BUILTINS].reverse().map((cmd) => (
                <span
                  key={cmd}
                  className="text-lg md:text-2xl font-mono tracking-tight whitespace-nowrap text-foreground"
                >
                  {cmd}
                </span>
              ))}
            </div>
          </Marquee>
        </section>

        {/* ═══════════════════════════════════════════════════════
            THE TERMINAL
            ═══════════════════════════════════════════════════════ */}
        <section
          id="shell"
          className="relative py-32 md:py-44 overflow-hidden"
        >
          <div
            className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] pointer-events-none"
            style={{
              background:
                "radial-gradient(circle, color-mix(in srgb, var(--pastel-blue) 6%, transparent), transparent 70%)",
              filter: "blur(80px)",
            }}
          />

          <div className="container mx-auto px-4 md:px-6 relative z-10">
            <ScrollReveal>
              <div className="max-w-3xl mx-auto text-center mb-20">
                <span className="inline-block text-xs uppercase tracking-[0.3em] text-[var(--pastel-blue)]/70 mb-6">
                  The Shell
                </span>
                <h2 className="text-4xl md:text-6xl lg:text-7xl font-black tracking-[-0.04em] leading-[0.9] mb-8">
                  Tab completion.{" "}
                  <span className="text-gradient-primary">Typed pipes.</span>
                  <br />
                  <span className="text-muted-foreground/30">
                    Directory labels.
                  </span>
                </h2>
                <p className="text-muted-foreground/60 text-lg md:text-xl leading-relaxed max-w-xl mx-auto">
                  Complete files, commands, and variables with tab. Search
                  history with Ctrl+R. Pipe OS command output into typed
                  functions. Write real code at the prompt.
                </p>
              </div>
            </ScrollReveal>

            <ScrollReveal delay={0.2}>
              <div className="max-w-4xl mx-auto">
                <AnimatedTerminal
                  lines={HERO_SESSION}
                  staggerDelay={0.1}
                  className="shadow-2xl shadow-black/50"
                />
              </div>
            </ScrollReveal>

            {/* Feature callouts */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 max-w-4xl mx-auto mt-12">
              {[
                { label: "History search", desc: "Ctrl+R reverse search" },
                { label: "Tab completion", desc: "Files, commands, types" },
                { label: "Hybrid pipes", desc: "OS → Jitzu functions" },
                { label: "Git-aware prompt", desc: "Branch + dirty status" },
              ].map((item, i) => (
                <ScrollReveal key={item.label} delay={0.3 + i * 0.08}>
                  <div className="text-center py-4">
                    <div className="text-sm font-medium text-foreground/80">
                      {item.label}
                    </div>
                    <div className="text-xs text-muted-foreground/40 mt-1">
                      {item.desc}
                    </div>
                  </div>
                </ScrollReveal>
              ))}
            </div>
          </div>
        </section>

        {/* ═══════════════════════════════════════════════════════
            LABELS — The killer feature nobody else has
            ═══════════════════════════════════════════════════════ */}
        <section className="relative py-24 md:py-32 overflow-hidden border-y border-white/[0.03]">
          <div className="container mx-auto px-4 md:px-6 relative z-10">
            <div className="grid lg:grid-cols-2 gap-12 max-w-5xl mx-auto items-center">
              <ScrollReveal>
                <div>
                  <span className="inline-block text-xs uppercase tracking-[0.3em] text-[var(--pastel-amber)]/70 mb-4">
                    Directory Labels
                  </span>
                  <h2 className="text-3xl md:text-5xl font-black tracking-[-0.04em] leading-[0.95] mb-6">
                    Name your directories.
                    <br />
                    <span className="text-muted-foreground/30">
                      Use them everywhere.
                    </span>
                  </h2>
                  <p className="text-muted-foreground/60 text-lg leading-relaxed mb-6">
                    No symlinks. No drive mapping. No editing fstab. Just give a
                    directory a name and use it as a path prefix — in cd, ls,
                    cat, anywhere.
                  </p>
                  <div className="space-y-3 text-sm text-muted-foreground/50">
                    <div className="flex items-start gap-3">
                      <div
                        className="w-1.5 h-1.5 rounded-full mt-1.5 shrink-0"
                        style={{ background: "var(--pastel-amber)" }}
                      />
                      <span>
                        Works as a path prefix:{" "}
                        <code className="text-foreground/70">
                          cd git:jitzu/site
                        </code>
                      </span>
                    </div>
                    <div className="flex items-start gap-3">
                      <div
                        className="w-1.5 h-1.5 rounded-full mt-1.5 shrink-0"
                        style={{ background: "var(--pastel-amber)" }}
                      />
                      <span>
                        Tab completes label names and paths after the colon
                      </span>
                    </div>
                    <div className="flex items-start gap-3">
                      <div
                        className="w-1.5 h-1.5 rounded-full mt-1.5 shrink-0"
                        style={{ background: "var(--pastel-amber)" }}
                      />
                      <span>
                        Saved to your config — available in every session
                      </span>
                    </div>
                  </div>
                </div>
              </ScrollReveal>

              <ScrollReveal delay={0.15}>
                <JitzuHighlighter
                  code={`// Label a directory
label git ~/git/

// Now use it anywhere
cd git:jitzu/site
ls git:jitzu/Tests/
cat git:jitzu/README.md

// List all your labels
labels
// git: → /home/simon/git/
// proj: → /home/simon/projects/

// Remove one
unlabel proj`}
                />
              </ScrollReveal>
            </div>
          </div>
        </section>

        {/* ═══════════════════════════════════════════════════════
            THE LANGUAGE
            ═══════════════════════════════════════════════════════ */}
        <section
          id="language"
          className="relative py-32 md:py-44 overflow-hidden"
        >
          <div className="container mx-auto px-4 md:px-6 relative z-10">
            <ScrollReveal>
              <div className="max-w-3xl mx-auto text-center mb-20">
                <span className="inline-block text-xs uppercase tracking-[0.3em] text-[var(--pastel-lavender)]/70 mb-6">
                  The Language
                </span>
                <h2 className="text-4xl md:text-6xl lg:text-7xl font-black tracking-[-0.04em] leading-[0.9] mb-8">
                  Oh, it's also
                  <br />
                  <span className="text-gradient-accent">
                    a full language.
                  </span>
                </h2>
                <p className="text-muted-foreground/60 text-lg md:text-xl leading-relaxed max-w-xl mx-auto">
                  Rust's enums. C#'s runtime. TypeScript's ergonomics. Compiled
                  to bytecode, running on .NET's VM.
                </p>
              </div>
            </ScrollReveal>

            <div className="max-w-4xl mx-auto space-y-6">
              {[
                {
                  title: "Pattern matching",
                  desc: "Match on types, destructure fields, guard with conditions.",
                  code: `let result = match get_user() {
    Ok(user) => \`Hello, {user.name}!\`,
    Err(e) => \`Failed: {e}\`,
}`,
                },
                {
                  title: "Result<T, E> and the ? operator",
                  desc: "Propagate errors without try/catch. Compose fallible operations.",
                  code: `fun load_config(): Result<Config, Error> {
    let file = try read_file("config.json")
    let parsed = try parse_json(file)
    return Ok(parsed)
}`,
                },
                {
                  title: "Traits and impl blocks",
                  desc: "Define shared behavior, implement it on any type.",
                  code: `trait Greet {
    fun greeting(self): String
}

impl Greet for User {
    fun greeting(self): String {
        \`Hey, {self.name}!\`
    }
}`,
                },
                {
                  title: "Types with type inference",
                  desc: "Define types with pub fields. Types are inferred from context.",
                  code: `type Point {
    pub x: Double,
    pub y: Double,
}

let origin = Point { x = 0.0, y = 0.0 }
let dist = (origin.x ** 2 + origin.y ** 2) ** 0.5`,
                },
                {
                  title: "NuGet packages",
                  desc: "Import any .NET package with a single line.",
                  code: `open "Newtonsoft.Json" as { JsonConvert }

let json = JsonConvert.serialize_object(user)
print(json)`,
                },
              ].map((feature, i) => (
                <ScrollReveal key={feature.title} delay={i * 0.08}>
                  <div className="grid md:grid-cols-2 gap-6 items-start">
                    <div className="pt-2">
                      <h3 className="text-lg font-semibold text-foreground mb-2">
                        {feature.title}
                      </h3>
                      <p className="text-sm text-muted-foreground/60 leading-relaxed">
                        {feature.desc}
                      </p>
                    </div>
                    <JitzuHighlighter code={feature.code} />
                  </div>
                </ScrollReveal>
              ))}
            </div>
          </div>
        </section>

        {/* ═══════════════════════════════════════════════════════
            CTA
            ═══════════════════════════════════════════════════════ */}
        <section className="relative py-32 md:py-44 flex items-center justify-center overflow-hidden">
          <div
            className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[500px] h-[300px] pointer-events-none"
            style={{
              background:
                "radial-gradient(ellipse, color-mix(in srgb, var(--pastel-lavender) 5%, transparent), transparent 70%)",
              filter: "blur(60px)",
            }}
          />

          <div className="relative z-10 text-center">
            <ScrollReveal>
              <div
                className="flex items-center justify-center gap-1 mb-12"
                style={{
                  fontFamily:
                    'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
                  fontSize: "1.1rem",
                }}
              >
                <span style={{ color: "#5faf5f", fontWeight: "bold" }}>
                  ❯{" "}
                </span>
                <motion.span
                  animate={{ opacity: [1, 0, 1] }}
                  transition={{
                    duration: 1.2,
                    repeat: Infinity,
                    ease: "linear",
                    times: [0, 0.5, 1],
                  }}
                  style={{ color: "var(--pastel-blue)" }}
                >
                  ▌
                </motion.span>
              </div>
            </ScrollReveal>

            <ScrollReveal delay={0.2}>
              <div className="flex flex-col sm:flex-row gap-4 justify-center items-center">
                <MagneticButton>
                  <Button
                    size="lg"
                    onClick={() =>
                      navigate({ to: "/docs/getting-started/installation" })
                    }
                    className="bg-[var(--pastel-blue)] hover:bg-[var(--pastel-blue)]/90 text-background font-medium gap-2 h-12 px-8"
                  >
                    <TerminalIcon className="size-4" />
                    Get Started
                    <ArrowRightIcon className="size-4" />
                  </Button>
                </MagneticButton>
                <MagneticButton>
                  <a href="https://github.com/simon-curtis/jitzu">
                    <Button
                      size="lg"
                      variant="outline"
                      className="glass-hover gap-2 h-12 px-8 text-muted-foreground/70 hover:text-foreground"
                    >
                      <GithubIcon className="size-4" />
                      View on GitHub
                    </Button>
                  </a>
                </MagneticButton>
              </div>
            </ScrollReveal>
          </div>
        </section>
      </main>

      {/* FOOTER */}
      <footer className="border-t border-white/[0.03] py-8">
        <div className="container mx-auto px-4 md:px-6">
          <div className="flex flex-col md:flex-row items-center justify-between gap-3 text-xs text-muted-foreground/30">
            <span className="font-medium text-gradient-primary text-sm">
              Jitzu
            </span>
            <span>
              Built on .NET 10 &middot; Bytecode compiled &middot; Open source
            </span>
          </div>
        </div>
      </footer>
    </div>
  );
}

export default Index;
