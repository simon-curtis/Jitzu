import { CodeBlock } from "@/components/code-block";
import { ScrollReveal } from "@/components/scroll-reveal";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/docs/")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground">
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tighter">Getting Started</h1>
        <p className="text-lg">
          Learn how to install and start using Jitzu in your projects.
        </p>
      </div>
      <ScrollReveal>
        <div className="space-y-4">
          <h2 className="text-2xl font-bold tracking-tighter">Installation</h2>
          <p className="leading-7">
            Only way atm is to build it from source from the{" "}
            <a
              href="https://github.com/simon-curtis/jitzu"
              className="text-blue-500 underline"
            >
              Jitzu Repo
            </a>{" "}
            which you don't have access to.
          </p>
          <p>
            Eventually it will be available via winget, as a dotnet tool, and via
            script.
          </p>
          {/* <CodeBlock language="bash" code="npm install -g Jitzu" /> */}
        </div>
      </ScrollReveal>
      <ScrollReveal>
        <div className="space-y-4">
          <h2 className="text-2xl font-bold tracking-tighter">
            Your First Program
          </h2>
          <p className="leading-7">
            Create a new file called `hello.jz` and write your first program:
          </p>
          <CodeBlock
            language="jitzu"
            code={`// This is a comment
print("Hello World")`}
          />
        </div>
      </ScrollReveal>
      <ScrollReveal>
        <div className="space-y-4">
          <h2 className="text-2xl font-bold tracking-tighter">
            Running Your Program
          </h2>
          <p className="leading-7">
            Use the jitzu cli tool `jz` to run your program
          </p>
          <CodeBlock language="bash" code={`jz run hello.jz`} />
        </div>
      </ScrollReveal>
    </article>
  );
}
