import { createFileRoute } from "@tanstack/react-router";
import exampleDoc from "../../../../docs/syntax/01_numbers.jz?raw";
import { CodeBlock } from "@/components/code-block";
import { ScrollReveal } from "@/components/scroll-reveal";
import { Heading1, Heading2 } from "@/components/ui/heading";

export const Route = createFileRoute("/docs/language/numbers")({
  component: RouteComponent,
});

const examples = exampleDoc
  .split(/^\/\/ /gm)
  .map((example) => {
    const splitAt = example.indexOf("\n");
    const title = example.slice(0, splitAt).trim();
    const code = example.slice(splitAt).trim();
    return { title, code };
  })
  .filter((example) => example.title && example.code);

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground">
      <Heading1>Numbers</Heading1>
      {examples.map((example) => {
        return (
          <ScrollReveal>
            <div>
              <Heading2>{example.title}</Heading2>
              <CodeBlock language="jitzu" code={example.code} />
            </div>
          </ScrollReveal>
        );
      })}
    </article>
  );
}
