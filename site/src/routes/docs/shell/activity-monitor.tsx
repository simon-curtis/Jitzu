import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/shell/activity-monitor")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Activity Monitor</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          The Jitzu shell includes a built-in full-screen activity monitor TUI, background job management,
          command timing, and process control commands.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>The Monitor Command</Heading2>
        <p>
          Run <code className="text-sm bg-muted px-2 py-1 rounded">monitor</code> to open a live-updating full-screen dashboard showing:
        </p>
        <ul className="list-disc pl-6 space-y-2">
          <li><strong>CPU usage</strong> — overall percentage with per-core sparkline bars and gradient coloring (green through red)</li>
          <li><strong>Memory usage</strong> — used/total with sparkline graph</li>
          <li><strong>Network</strong> — send/receive rates with sparkline history</li>
          <li><strong>Disk usage</strong> — per-drive gauge bars</li>
          <li><strong>Process list</strong> — navigable tree view with PID, CPU%, memory, and command name</li>
        </ul>

        <Heading3>Monitor Controls</Heading3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Key</th>
                <th className="text-left py-2 font-medium">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>q / Escape</code></td><td className="py-2">Quit the monitor</td></tr>
              <tr><td className="py-2 pr-4"><code>Up / Down</code></td><td className="py-2">Navigate process list</td></tr>
              <tr><td className="py-2 pr-4"><code>k</code></td><td className="py-2">Send SIGTERM to selected process</td></tr>
              <tr><td className="py-2 pr-4"><code>kk</code> (double tap)</td><td className="py-2">Send SIGKILL to selected process</td></tr>
              <tr><td className="py-2 pr-4"><code>/</code></td><td className="py-2">Filter processes by name</td></tr>
              <tr><td className="py-2 pr-4"><code>f</code></td><td className="py-2">Toggle: show only shell child processes</td></tr>
            </tbody>
          </table>
        </div>
        <p>
          The monitor renders with bordered panels, gradient sparklines, and color-coded metrics.
          It refreshes automatically and adapts to terminal size changes.
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Background Jobs</Heading2>
        <p>
          Append <code className="text-sm bg-muted px-2 py-1 rounded">&</code> to any command to run it in the background.
          The shell returns a job ID and process ID immediately, and the prompt stays responsive.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ dotnet build &
[1] 12345

❯ sleep 10 &
[2] 12346`}
        />

        <Heading3>Managing Jobs</Heading3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>jobs</code></td><td className="py-2">List all background jobs with status (Running/Done)</td></tr>
              <tr><td className="py-2 pr-4"><code>fg [%id]</code></td><td className="py-2">Bring a job to the foreground (waits for completion, shows output)</td></tr>
              <tr><td className="py-2 pr-4"><code>bg</code></td><td className="py-2">List background jobs (alias for jobs)</td></tr>
            </tbody>
          </table>
        </div>

        <CodeBlock
          language="bash"
          code={`❯ jobs
[1]  Running    dotnet build
[2]  Done       sleep 10

❯ fg %1
[1]  dotnet build
Build succeeded.
    0 Warning(s)
    0 Error(s)`}
        />
        <p>
          Completed jobs are reported automatically at the next prompt. The prompt also shows the count of
          active background jobs (e.g. <code className="text-sm bg-muted px-2 py-1 rounded">[2]</code>).
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Command Timing</Heading2>
        <p>
          Use <code className="text-sm bg-muted px-2 py-1 rounded">time</code> to measure how long a command takes to execute:
        </p>
        <CodeBlock
          language="bash"
          code={`❯ time dotnet build
Build succeeded.

real    0m2.345s`}
        />
        <p>
          Commands that take longer than 2 seconds also show their duration in the prompt
          line automatically (e.g. <code className="text-sm bg-muted px-2 py-1 rounded">took 5s</code>).
        </p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Watch</Heading2>
        <p>
          The <code className="text-sm bg-muted px-2 py-1 rounded">watch</code> command repeats a command at regular intervals.
          Press <code className="text-sm bg-muted px-2 py-1 rounded">Ctrl+C</code> to stop.
        </p>
        <CodeBlock
          language="bash"
          code={`❯ watch -n 5 date
Every 5.0s: date

Sat Feb 14 14:23:00 2026`}
        />
        <p>Default interval is 2 seconds. Use <code className="text-sm bg-muted px-2 py-1 rounded">-n</code> to specify a different interval.</p>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Process Management</Heading2>

        <div className="overflow-x-auto">
          <table className="w-full text-sm border-collapse">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-2 pr-4 font-medium">Command</th>
                <th className="text-left py-2 font-medium">Description</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              <tr><td className="py-2 pr-4"><code>kill [-9] pid</code></td><td className="py-2">Kill a process by PID or <code>%jobid</code></td></tr>
              <tr><td className="py-2 pr-4"><code>killall [-9] name</code></td><td className="py-2">Kill all processes matching a name</td></tr>
            </tbody>
          </table>
        </div>

        <CodeBlock
          language="bash"
          code={`❯ kill %1
[1]  Terminated  dotnet build

❯ kill 12345
Process 12345 terminated.

❯ kill -9 12345
Process 12345 killed.

❯ killall node
Killed 3 process(es) named 'node'.`}
        />
        <p>
          The <code className="text-sm bg-muted px-2 py-1 rounded">-9</code> flag sends SIGKILL (force kill) instead of SIGTERM (graceful termination).
          Use <code className="text-sm bg-muted px-2 py-1 rounded">%id</code> to reference background jobs by their job ID.
        </p>
      </section>
      </ScrollReveal>
    </article>
  );
}
