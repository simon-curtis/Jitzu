import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/shell/overview")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Jitzu Shell</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          The Jitzu shell is a feature-rich interactive environment that blends a Unix-style command line
          with the full Jitzu language. You can run shell commands, write Jitzu expressions, and pipe
          between them — all in one place.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Launching the Shell</Heading2>
        <p>Start the shell with the <code className="text-sm bg-muted px-2 py-1 rounded">jzsh</code> command:</p>
        <CodeBlock
          language="bash"
          code={`$ jzsh

jzsh v0.3.0

• runtime    : 10.0.0
• config     : ~/.jitzu/config.jz
• platform   : Unix

Type \`help\` to get started.

simon@dev ~/projects/api (main) *              14:23
❯`}
        />
        <p>
          The prompt shows your username, hostname, current directory (relative to the git root if inside a repo),
          git branch with status indicators (<code className="text-sm bg-muted px-2 py-1 rounded">*</code> dirty, <code className="text-sm bg-muted px-2 py-1 rounded">+</code> staged, <code className="text-sm bg-muted px-2 py-1 rounded">?</code> untracked), and the current time.
        </p>
        <p>
          Use <code className="text-sm bg-muted px-2 py-1 rounded">--no-splash</code> to skip the welcome banner.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Mixing Code and Commands</Heading2>
        <p>
          The shell automatically detects whether your input is Jitzu code or a shell command.
          Jitzu code is tried first; if parsing fails, the input is executed as an OS command.
        </p>

        <Heading3>Jitzu Expressions</Heading3>
        <CodeBlock
          language="jitzu"
          code={`❯ 1 + 1
2

❯ let greeting = "hello from the shell"
❯ greeting.to_upper()
"HELLO FROM THE SHELL"

❯ fun factorial(n: Int): Int {
|     if n <= 1 { 1 } else { n * factorial(n - 1) }
| }
❯ factorial(10)
3628800`}
        />

        <Heading3>OS Commands</Heading3>
        <p>
          Commands that aren't valid Jitzu code fall through to the system shell.
          Many common commands like <code className="text-sm bg-muted px-2 py-1 rounded">ls</code>, <code className="text-sm bg-muted px-2 py-1 rounded">grep</code>, and <code className="text-sm bg-muted px-2 py-1 rounded">find</code> are
          implemented as builtins with colored output, but any program on your PATH works too:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ git log --oneline -3
a1b2c3d Add pattern matching support
b2c3d4e Fix lexer edge case
c3d4e5f Initial commit

❯ dotnet build
Build succeeded.`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Command Substitution</Heading2>
        <p>
          Use <code className="text-sm bg-muted px-2 py-1 rounded">$(command)</code> to capture the output of a command and insert it inline.
          Nesting is supported.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ echo "I am in $(pwd)"
I am in /home/simon/projects/api

❯ echo "Branch: $(git branch --show-current)"
Branch: main`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Environment Variables</Heading2>
        <p>
          Shell-style variable expansion works with <code className="text-sm bg-muted px-2 py-1 rounded">$VAR</code> and <code className="text-sm bg-muted px-2 py-1 rounded">{"${VAR}"}</code> syntax.
          Use <code className="text-sm bg-muted px-2 py-1 rounded">export</code> to set and <code className="text-sm bg-muted px-2 py-1 rounded">unset</code> to remove environment variables.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ export EDITOR=nvim
❯ echo $EDITOR
nvim

❯ echo \${HOME}
/home/simon`}
        />
        <p>
          Single-quoted strings suppress expansion: <code className="text-sm bg-muted px-2 py-1 rounded">echo '$HOME'</code> prints the literal text <code className="text-sm bg-muted px-2 py-1 rounded">$HOME</code>.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Multi-line Input</Heading2>
        <p>
          When a line ends with an unclosed brace, the shell continues reading on the next line
          with a <code className="text-sm bg-muted px-2 py-1 rounded">|</code> continuation prompt:
        </p>
        <CodeBlock
          language="jitzu"
          code={`❯ let person = {
|   name = "Alice",
|   age = 30,
|   address = {
|     city = "New York",
|     zip = "10001"
|   }
| }
❯ person.address.city
"New York"`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Glob Expansion</Heading2>
        <p>
          Arguments containing <code className="text-sm bg-muted px-2 py-1 rounded">*</code> or <code className="text-sm bg-muted px-2 py-1 rounded">?</code> are
          expanded to matching file paths. Recursive <code className="text-sm bg-muted px-2 py-1 rounded">**</code> patterns are also supported.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ ls *.cs
Program.cs  Lexer.cs  Parser.cs

❯ find src/**/*.cs
src/Core/Compiler.cs
src/Core/Interpreter.cs
src/Runtime/Stack.cs`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Sourcing Scripts</Heading2>
        <p>
          The <code className="text-sm bg-muted px-2 py-1 rounded">source</code> command (or its shorthand <code className="text-sm bg-muted px-2 py-1 rounded">.</code>) executes a <code className="text-sm bg-muted px-2 py-1 rounded">.jz</code> file
          line-by-line in the current session, so any variables or functions it defines remain available:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ source ~/scripts/helpers.jz
❯ my_helper_function("test")
"processed: test"`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Introspection</Heading2>
        <p>The shell includes commands to inspect the current session state:</p>
        <ul className="list-disc pl-6 space-y-2">
          <li><code className="text-sm bg-muted px-2 py-1 rounded">vars</code> — list all defined variables and their types</li>
          <li><code className="text-sm bg-muted px-2 py-1 rounded">types</code> — show available types (built-in, user-defined, imported)</li>
          <li><code className="text-sm bg-muted px-2 py-1 rounded">functions</code> — show defined functions</li>
          <li><code className="text-sm bg-muted px-2 py-1 rounded">reset</code> — clear all session state and start fresh</li>
        </ul>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Error Handling</Heading2>
        <p>Errors are displayed gracefully without crashing the shell. The prompt arrow turns red after a failed command:</p>
        <CodeBlock
          language="jitzu"
          code={`❯ 10 / 0
Error: Division by zero

❯ cd nonexistent
No such directory: nonexistent
  Did you mean src/?`}
        />
      </section>
      </ScrollReveal>
    </article>
  );
}
