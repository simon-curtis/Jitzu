import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/shell/completion-and-editing")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Completion & Editing</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          The Jitzu shell includes a custom readline implementation with tab completion, history
          predictions, reverse search, text selection, and full line-editing support.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Tab Completion</Heading2>
        <p>
          Press <strong>Tab</strong> to trigger context-aware completions. The shell completes:
        </p>
        <ul className="list-disc pl-6 space-y-2">
          <li><strong>File paths</strong> — files and directories in the current directory, with tilde (<code className="text-sm bg-muted px-2 py-1 rounded">~</code>) and label expansion</li>
          <li><strong>Built-in commands</strong> — all 60+ shell builtins</li>
          <li><strong>Executables on PATH</strong> — programs available in your system PATH</li>
          <li><strong>Jitzu identifiers</strong> — variables, functions, and types from the current session</li>
          <li><strong>Label paths</strong> — type a label prefix (e.g. <code className="text-sm bg-muted px-2 py-1 rounded">git:</code>) and Tab completes paths within the labeled directory</li>
        </ul>

        <Heading3>Completion Workflow</Heading3>
        <p>
          If there's a single match, it's applied immediately. If there are multiple matches,
          a dropdown appears. Press <strong>Tab</strong> again to accept the highlighted completion, or use
          <strong> Up/Down</strong> arrows to navigate the dropdown.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ gre⇥         → grep
❯ src/Pa⇥      → src/Parser.cs
❯ git:proj⇥    → git:projects/`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>History Predictions</Heading2>
        <p>
          As you type, the shell shows predictions from your command history. The top prediction
          appears as dimmed ghost text after your cursor. Below that, a dropdown shows up to 5
          matching history entries.
        </p>
        <ul className="list-disc pl-6 space-y-2">
          <li><strong>Right Arrow</strong> — accept the ghost text prediction</li>
          <li><strong>Up/Down Arrows</strong> — navigate the predictions dropdown</li>
          <li><strong>Enter</strong> — accept the selected prediction</li>
          <li><strong>Delete</strong> — remove the selected entry from history</li>
          <li><strong>Escape</strong> — dismiss predictions</li>
        </ul>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Reverse History Search</Heading2>
        <p>
          Press <strong>Ctrl+R</strong> to enter reverse search mode. Type a substring to search
          backward through history. Press <strong>Ctrl+R</strong> again to cycle to the next match.
        </p>
        <CodeBlock
          language="bash"
          code={`(reverse-search): build
→ dotnet build --configuration Release`}
        />
        <p>
          Press <strong>Enter</strong> to execute the match, <strong>Escape</strong> to cancel, or any
          other key to exit search mode and edit the matched line.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>History Navigation</Heading2>
        <p>
          When no dropdown is visible, the Up and Down arrow keys navigate through your full command history.
          History is persisted to disk across sessions.
        </p>
        <ul className="list-disc pl-6 space-y-2">
          <li><strong>Up Arrow</strong> — previous command in history</li>
          <li><strong>Down Arrow</strong> — next command in history</li>
          <li><code className="text-sm bg-muted px-2 py-1 rounded">history</code> — display full command history with line numbers</li>
        </ul>
        <p>
          Duplicate commands are deduplicated — only the most recent occurrence is kept.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Line Editing</Heading2>

        <Heading3>Cursor Movement</Heading3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Shortcut</th>
                <th className="text-left py-2 font-medium">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>Left / Right</code></td><td className="py-2">Move cursor one character</td></tr>
              <tr><td className="py-2 pr-4"><code>Ctrl+Left / Ctrl+Right</code></td><td className="py-2">Jump by word</td></tr>
              <tr><td className="py-2 pr-4"><code>Home / Ctrl+A</code></td><td className="py-2">Move to beginning of line</td></tr>
              <tr><td className="py-2 pr-4"><code>End</code></td><td className="py-2">Move to end of line</td></tr>
            </tbody>
          </table>
        </div>

        <Heading3>Editing</Heading3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Shortcut</th>
                <th className="text-left py-2 font-medium">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>Backspace</code></td><td className="py-2">Delete character before cursor</td></tr>
              <tr><td className="py-2 pr-4"><code>Ctrl+Backspace</code></td><td className="py-2">Delete word before cursor</td></tr>
              <tr><td className="py-2 pr-4"><code>Delete</code></td><td className="py-2">Delete character after cursor</td></tr>
            </tbody>
          </table>
        </div>

        <Heading3>Text Selection</Heading3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Shortcut</th>
                <th className="text-left py-2 font-medium">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>Shift+Left / Shift+Right</code></td><td className="py-2">Extend selection by character</td></tr>
              <tr><td className="py-2 pr-4"><code>Shift+Home / Shift+End</code></td><td className="py-2">Select to beginning/end of line</td></tr>
              <tr><td className="py-2 pr-4"><code>Ctrl+C</code> (with selection)</td><td className="py-2">Copy selection to clipboard</td></tr>
            </tbody>
          </table>
        </div>
        <p>
          Typing or pasting while text is selected replaces the selected region.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Other Shortcuts</Heading2>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Shortcut</th>
                <th className="text-left py-2 font-medium">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>Ctrl+C</code> (no selection)</td><td className="py-2">Cancel current line</td></tr>
              <tr><td className="py-2 pr-4"><code>Ctrl+R</code></td><td className="py-2">Reverse history search</td></tr>
              <tr><td className="py-2 pr-4"><code>Escape</code></td><td className="py-2">Dismiss predictions/completions</td></tr>
            </tbody>
          </table>
        </div>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Syntax Highlighting</Heading2>
        <p>
          The shell prompt applies live syntax coloring as you type, highlighting commands, keywords,
          strings, flags, pipe operators, and boolean values. Colors are configurable via the{" "}
          <a href="/docs/shell/customization" className="text-primary hover:underline">
            theme system
          </a>.
        </p>
      </section>
      </ScrollReveal>
    </article>
  );
}
