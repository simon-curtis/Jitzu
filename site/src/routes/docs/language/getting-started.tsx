import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { ScrollReveal } from "@/components/scroll-reveal";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";

export const Route = createFileRoute("/docs/language/getting-started")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Getting Started with Jitzu</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          Welcome to Jitzu, a modern scripting language designed to be "Fast Enough™, Overengineered, and Unoriginal" - packed with syntax sugar to make scripting fun and expressive.
        </p>
      </div>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>What is Jitzu?</Heading2>
          <p>
            Jitzu is a modern scripting language that combines features from Rust, C#, F#, Go, TypeScript, Scala, and Zig.
            It's designed for both script execution and interactive shell usage, prioritizing developer productivity and expressiveness.
          </p>
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Design Philosophy</Heading2>
          <p>Jitzu is designed to be:</p>
          <ul className="list-disc pl-6 space-y-2">
            <li><strong>Fast Enough™</strong> - Optimized for developer productivity over raw performance</li>
            <li><strong>Overengineered</strong> - Includes many language features for expressiveness</li>
            <li><strong>Unoriginal</strong> - Borrows the best ideas from other languages</li>
            <li><strong>Fun to use</strong> - Syntax sugar makes common tasks enjoyable</li>
          </ul>
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Your First Jitzu Program</Heading2>
          <p>Let's start with the classic "Hello, World!" program:</p>
          <CodeBlock
            language="jitzu"
            code={`// Simple Hello World
print("Hello, World!")`}
          />
          <p>Save this as <code className="text-sm bg-muted px-2 py-1 rounded">hello.jz</code> and run it with the Jitzu interpreter.</p>
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>String Interpolation</Heading2>
          <p>One of Jitzu's most convenient features is string interpolation using template literals:</p>
          <CodeBlock
            language="jitzu"
            code={`// With string interpolation
let name = "Jitzu"
print(\`Hello from {name}!\`)

// You can use expressions inside interpolation
let version = 1.0
print(\`Welcome to {name} version {version}!\`)`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Basic Program Structure</Heading2>
          <p>Jitzu programs are collections of expressions and statements:</p>
          <CodeBlock
            language="jitzu"
            code={`// Variables
let greeting = "Hello"
let language = "Jitzu"

// Function definition
fun welcome(name: String): String {
    \`{greeting} from {language}, {name}!\`
}

// Using the function
let message = welcome("Developer")
print(message)`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Key Language Features at a Glance</Heading2>

          <Heading3>Mutable and Immutable Variables</Heading3>
          <p>Use <code>mut</code> for mutable data:</p>
          <CodeBlock
            language="jitzu"
            code={`let x = 42          // Immutable
let mut counter = 0 // Mutable
counter += 1`}
          />

          <Heading3>Everything is an Expression</Heading3>
          <p>Functions automatically return the last expression:</p>
          <CodeBlock
            language="jitzu"
            code={`fun add(a: Int, b: Int): Int {
    a + b  // No 'return' needed
}`}
          />

          <Heading3>Strong Type System with Inference</Heading3>
          <p>Types are inferred when possible, but you can be explicit:</p>
          <CodeBlock
            language="jitzu"
            code={`let age = 25              // Inferred as Int
let name: String = "Alice" // Explicit type`}
          />

          <Heading3>Pattern Matching</Heading3>
          <p>Match expressions for control flow:</p>
          <CodeBlock
            language="jitzu"
            code={`union Shape {
    Circle(Double),
    Square(Double),
}

let description = match shape {
    Shape.Circle(r) => \`Circle with radius {r}\`,
    Shape.Square(s) => \`Square with side {s}\`,
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Interactive Shell Mode</Heading2>
          <p>Jitzu includes a REPL (Read-Eval-Print Loop) for quick experimentation:</p>
          <CodeBlock
            language="bash"
            code={`$ jzsh

~ 1 + 1
2

~ let name = "Jitzu"
~ \`Hello from {name}!\`
"Hello from Jitzu!"

~ for i in 1..=3 { print(\`Number {i}\`) }
Number 1
Number 2
Number 3`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Quick Reference</Heading2>

          <Heading3>Common Operations</Heading3>
          <CodeBlock
            language="jitzu"
            code={`// Variables
let x = 42
let mut y = 10

// String interpolation
print(\`x = {x}, y = {y}\`)

// Basic arithmetic
let sum = x + y
let difference = x - y

// Conditionals
if x > y {
    print("x is greater")
}

// Loops
for i in 1..=5 {
    print(\`Count: {i}\`)
}`}
          />

          <Heading3>File Structure</Heading3>
          <ul className="list-disc pl-6 space-y-2">
            <li>Source files use <code>.jz</code> extension</li>
            <li>Use <code>open</code> statements for imports</li>
            <li>Programs start executing from top-level expressions</li>
          </ul>
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Next Steps</Heading2>
          <p>Now that you've seen the basics, explore these areas to learn more:</p>
          <ul className="list-disc pl-6 space-y-2">
            <li><a href="/docs/language/basic-syntax" className="text-primary hover:underline">Basic Syntax</a> - Comments, variables, and imports</li>
            <li><a href="/docs/language/data-types" className="text-primary hover:underline">Data Types</a> - Numbers, strings, vectors, and more</li>
            <li><a href="/docs/language/functions" className="text-primary hover:underline">Functions</a> - Function definition and error handling</li>
            <li><a href="/docs/language/control-flow" className="text-primary hover:underline">Control Flow</a> - Conditionals and loops</li>
            <li><a href="/docs/language/object-oriented" className="text-primary hover:underline">Types</a> - Custom types and composition</li>
          </ul>
        </section>
      </ScrollReveal>
    </article>
  );
}
