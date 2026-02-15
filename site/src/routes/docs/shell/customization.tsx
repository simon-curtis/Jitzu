import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/shell/customization")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Customization</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          The Jitzu shell supports theme customization, startup configuration, aliases, and path labels
          to personalize your workflow.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Theme Configuration</Heading2>
        <p>
          Colors are controlled by <code className="text-sm bg-muted px-2 py-1 rounded">~/.jitzu/colours.json</code>.
          A default configuration is written automatically on first launch. All values are hex color strings (e.g. <code className="text-sm bg-muted px-2 py-1 rounded">#87af87</code>).
        </p>
        <CodeBlock
          language="json"
          code={`{
  "syntax": {
    "command": "#87af87",
    "keyword": "#d7afaf",
    "string": "#afaf87",
    "flag": "#87afaf",
    "pipe": "#af87af",
    "boolean": "#d7af87"
  },
  "git": {
    "branch": "#808080",
    "dirty": "#d7af87",
    "staged": "#87af87",
    "untracked": "#808080",
    "remote": "#87afaf"
  },
  "prompt": {
    "directory": "#87d7ff",
    "arrow": "#5faf5f",
    "error": "#d75f5f",
    "user": "#5f8787",
    "duration": "#d7af87",
    "time": "#808080",
    "jobs": "#87afaf"
  },
  "ls": {
    "directory": "#87afd7",
    "executable": "#87af87",
    "archive": "#d75f5f",
    "media": "#af87af",
    "code": "#87afaf",
    "config": "#d7af87",
    "project": "#d7af87",
    "size": "#87af87",
    "dim": "#808080"
  },
  "error": "#d75f5f",
  "prediction": {
    "text": "#808080",
    "selected": {
      "bg": "#303050",
      "fg": "#ffffff"
    }
  },
  "selection": {
    "bg": "#264f78",
    "fg": "#ffffff"
  },
  "dropdown": {
    "gutter": "#404040",
    "status": "#5f87af"
  }
}`}
        />

        <Heading3>Color Categories</Heading3>
        <ul className="list-disc pl-6 space-y-2">
          <li><strong>syntax</strong> — live input highlighting (commands, keywords, strings, flags, pipes, booleans)</li>
          <li><strong>git</strong> — prompt git status indicators (branch, dirty, staged, untracked, remote ahead/behind)</li>
          <li><strong>prompt</strong> — prompt elements (directory, arrow, error state, user, duration, clock, job count)</li>
          <li><strong>ls</strong> — file listing colors by type (directories, executables, archives, media, code, config, project files)</li>
          <li><strong>prediction</strong> — history prediction ghost text and dropdown selection</li>
          <li><strong>selection</strong> — text selection highlight</li>
          <li><strong>dropdown</strong> — completion dropdown gutter and status bar</li>
        </ul>
        <p>
          Edit any value and restart the shell to apply changes. Malformed entries are silently
          ignored and fall back to defaults.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Startup Configuration</Heading2>
        <p>
          The file <code className="text-sm bg-muted px-2 py-1 rounded">~/.jitzu/config.jz</code> is executed automatically when the shell starts,
          similar to <code className="text-sm bg-muted px-2 py-1 rounded">.bashrc</code> or <code className="text-sm bg-muted px-2 py-1 rounded">.zshrc</code>.
          Each line is run as a shell command, so you can set aliases, labels, environment variables,
          and define Jitzu functions that will be available in every session.
        </p>
        <CodeBlock
          language="jitzu"
          code={`// ~/.jitzu/config.jz

// Set up aliases
alias ll="ls -la"
alias gs="git status"
alias gp="git push"

// Set up path labels
label git ~/git
label docs ~/Documents

// Set environment variables
export EDITOR=nvim
export DOTNET_CLI_TELEMETRY_OPTOUT=1

// Define helper functions
fun greet(): String {
    \`Good morning, {whoami}!\`
}`}
        />
        <p>
          Lines starting with <code className="text-sm bg-muted px-2 py-1 rounded">//</code> are treated as comments and skipped.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Aliases</Heading2>
        <p>
          Aliases map short names to longer commands. They persist to disk across sessions
          and are automatically expanded when the alias name appears as the first word of a command.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ alias ll="ls -la"
Alias set: ll → ls -la

❯ ll
d-----       -  Jan 15 09:44  src/
-a-r--   2.5K  Jan 15 14:23  Program.cs

❯ aliases
ll → ls -la
gs → git status

❯ unalias ll
Alias removed: ll`}
        />
        <p>
          Aliases are stored in the application data directory
          (e.g. <code className="text-sm bg-muted px-2 py-1 rounded">~/.local/share/Jitzu/aliases.txt</code> on Linux
          or <code className="text-sm bg-muted px-2 py-1 rounded">%APPDATA%/Jitzu/aliases.txt</code> on Windows).
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Path Labels</Heading2>
        <p>
          Labels map short names to directory paths. Use a label by appending a colon, e.g. <code className="text-sm bg-muted px-2 py-1 rounded">git:</code>.
          Labels work in <code className="text-sm bg-muted px-2 py-1 rounded">cd</code>, file arguments, and tab completion.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ label api ~/projects/api
Label set: api → /home/simon/projects/api

❯ cd api:
❯ pwd
/home/simon/projects/api

❯ cd api:src/controllers
❯ cat api:README.md

❯ labels
api → /home/simon/projects/api
docs → /home/simon/Documents

❯ unlabel api
Label removed: api`}
        />
        <p>
          Labels expand transparently in file arguments, so commands
          like <code className="text-sm bg-muted px-2 py-1 rounded">cat api:src/Program.cs</code> and <code className="text-sm bg-muted px-2 py-1 rounded">ls api:</code> work
          as expected. Tab completion also understands label prefixes.
        </p>
      </section>
      </ScrollReveal>
    </article>
  );
}
