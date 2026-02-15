import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/getting-started/installation")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Installation</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          Jitzu is distributed as a single binary. You can choose between a self-contained build
          (larger, no prerequisites) or a framework-dependent build (smaller, requires .NET 10).
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Self-Contained Binary</Heading2>
        <p>
          The self-contained binary bundles the .NET runtime, so it works on any machine with no
          prerequisites. This is the easiest way to get started.
        </p>

        <Heading3>Windows</Heading3>
        <p>
          Download <code className="text-sm bg-muted px-2 py-1 rounded">jitzu-win-x64.zip</code> from the{" "}
          <a href="https://github.com/simon-curtis/jitzu/releases/latest" className="text-primary hover:underline">
            latest GitHub release
          </a>{" "}
          and extract it. Add the extracted directory to your PATH, or move the binary to a directory already on your PATH.
        </p>
        <CodeBlock
          language="bash"
          code={`# PowerShell
Expand-Archive jitzu-win-x64.zip -DestinationPath C:\\tools\\jitzu
$env:PATH += ";C:\\tools\\jitzu"

# Or move to a directory already on PATH
Move-Item jitzu.exe C:\\Windows\\`}
        />

        <Heading3>macOS</Heading3>
        <p>
          Download <code className="text-sm bg-muted px-2 py-1 rounded">jitzu-osx-arm64.tar.gz</code> (Apple Silicon)
          or <code className="text-sm bg-muted px-2 py-1 rounded">jitzu-osx-x64.tar.gz</code> (Intel) from the{" "}
          <a href="https://github.com/simon-curtis/jitzu/releases/latest" className="text-primary hover:underline">
            latest release
          </a>.
        </p>
        <CodeBlock
          language="bash"
          code={`tar xzf jitzu-osx-arm64.tar.gz
sudo mv jitzu /usr/local/bin/
chmod +x /usr/local/bin/jitzu`}
        />

        <Heading3>Linux</Heading3>
        <p>
          Download <code className="text-sm bg-muted px-2 py-1 rounded">jitzu-linux-x64.tar.gz</code> from the{" "}
          <a href="https://github.com/simon-curtis/jitzu/releases/latest" className="text-primary hover:underline">
            latest release
          </a>.
        </p>
        <CodeBlock
          language="bash"
          code={`tar xzf jitzu-linux-x64.tar.gz
sudo mv jitzu /usr/local/bin/
chmod +x /usr/local/bin/jitzu`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Framework-Dependent</Heading2>
        <p>
          The framework-dependent build is smaller but requires the{" "}
          <a href="https://dotnet.microsoft.com/download/dotnet/10.0" className="text-primary hover:underline">
            .NET 10 runtime
          </a>{" "}
          to be installed on your system.
        </p>
        <p>
          Download the <code className="text-sm bg-muted px-2 py-1 rounded">jitzu-framework-dependent</code> archive
          for your platform from the{" "}
          <a href="https://github.com/simon-curtis/jitzu/releases/latest" className="text-primary hover:underline">
            latest release
          </a>{" "}
          and follow the same extraction steps as above.
        </p>
        <CodeBlock
          language="bash"
          code={`# Verify .NET 10 is installed
dotnet --version
# Should output 10.x.x

# Extract and install
tar xzf jitzu-framework-dependent-linux-x64.tar.gz
sudo mv jitzu /usr/local/bin/`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Build from Source</Heading2>
        <p>
          Jitzu requires the{" "}
          <a href="https://dotnet.microsoft.com/download/dotnet/10.0" className="text-primary hover:underline">
            .NET 10 SDK
          </a>{" "}
          to build from source.
        </p>
        <CodeBlock
          language="bash"
          code={`git clone https://github.com/simon-curtis/jitzu.git
cd jitzu
dotnet build
dotnet run --project Jitzu.Interpreter -- --help`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Verify Installation</Heading2>
        <p>
          After installing, verify everything works:
        </p>
        <CodeBlock
          language="bash"
          code={`$ jitzu --version
jitzu 0.3.0

$ jzsh
jzsh v0.3.0

• runtime    : 10.0.0
• config     : ~/.jitzu/config.jz
• platform   : Unix

Type \`help\` to get started.

❯ 1 + 1
2`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <p className="text-muted-foreground">
          Release binaries will be published automatically via GitHub Actions.
          Until CI is configured, you can build from source using the instructions above.
        </p>
      </section>
      </ScrollReveal>
    </article>
  );
}
