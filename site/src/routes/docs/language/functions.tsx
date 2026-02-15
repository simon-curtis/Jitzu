import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { ScrollReveal } from "@/components/scroll-reveal";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";

export const Route = createFileRoute("/docs/language/functions")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Functions</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          Functions are the building blocks of Jitzu programs. They support type annotations,
          automatic return of the last expression, recursion, and Result-based error handling.
        </p>
      </div>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Function Definition</Heading2>

          <Heading3>Basic Function Syntax</Heading3>
          <CodeBlock
            language="jitzu"
            code={`// Basic function with explicit return type
fun add(a: Int, b: Int): Int {
    a + b
}

// Function with string return
fun greet(name: String): String {
    \`Hello, {name}!\`
}

// Function with no parameters
fun get_pi(): Double {
    3.14159
}

// Using functions
print(add(5, 3))        // 8
print(greet("World"))   // "Hello, World!"
print(get_pi())         // 3.14159`}
          />

          <Heading3>Everything is an Expression</Heading3>
          <p>Functions automatically return the value of their last expression - no <code>return</code> keyword needed:</p>
          <CodeBlock
            language="jitzu"
            code={`fun calculate_area(radius: Double): Double {
    let pi = 3.14159
    pi * radius * radius  // This value is automatically returned
}

// You can use explicit return for early exits
fun classify(n: Int): String {
    if n < 0 {
        return "negative"
    }
    if n == 0 {
        return "zero"
    }
    "positive"
}`}
          />

          <Heading3>Function Parameters</Heading3>
          <CodeBlock
            language="jitzu"
            code={`// Required parameters with types
fun format_name(first: String, last: String): String {
    \`{first} {last}\`
}

// Parameters are typed
fun distance(x1: Double, y1: Double, x2: Double, y2: Double): Double {
    let dx = x2 - x1
    let dy = y2 - y1
    (dx * dx + dy * dy)
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Recursive Functions</Heading2>
          <p>Jitzu supports recursive functions:</p>

          <CodeBlock
            language="jitzu"
            code={`// Classic recursive factorial
fun factorial(n: Int): Int {
    if n <= 1 {
        1
    } else {
        n * factorial(n - 1)
    }
}

// Power function
fun power(base: Int, exp: Int): Int {
    if exp == 0 {
        1
    } else {
        base * power(base, exp - 1)
    }
}

// Fibonacci
fun fibonacci(n: Int): Int {
    if n <= 1 {
        n
    } else {
        fibonacci(n - 1) + fibonacci(n - 2)
    }
}

print(factorial(5))     // 120
print(power(2, 8))      // 256
print(fibonacci(10))    // 55`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Error Handling with Result Types</Heading2>

          <Heading3>Returning Results</Heading3>
          <p>Use <code>Result&lt;T, E&gt;</code> for functions that can fail:</p>
          <CodeBlock
            language="jitzu"
            code={`fun divide(a: Double, b: Double): Result<Double, String> {
    if b == 0.0 {
        Err("Division by zero")
    } else {
        Ok(a / b)
    }
}

// Pattern matching on Results
match divide(10.0, 2.0) {
    Ok(result) => print(\`Result: {result}\`),
    Err(error) => print(\`Error: {error}\`)
}`}
          />

          <Heading3>The Try Operator</Heading3>
          <p>Use <code>try</code> to propagate errors early:</p>
          <CodeBlock
            language="jitzu"
            code={`fun safe_sqrt(x: Double): Result<Double, String> {
    if x < 0.0 {
        Err("Cannot take square root of negative number")
    } else {
        Ok(x)
    }
}

// try returns Err early if the operation fails
fun complex_calculation(a: Double, b: Double): Result<Double, String> {
    let step1 = try divide(a, b)
    let step2 = try safe_sqrt(step1)
    Ok(step2)
}

match complex_calculation(100.0, 4.0) {
    Ok(value) => print(\`Result: {value}\`),
    Err(error) => print(\`Failed: {error}\`)
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Best Practices</Heading2>

          <ul className="list-disc pl-6 space-y-2">
            <li><strong>Single responsibility</strong> - Each function should do one thing well</li>
            <li><strong>Descriptive names</strong> - Use clear, meaningful function names</li>
            <li><strong>Small functions</strong> - Keep functions focused and concise</li>
            <li>Prefer <code>Result&lt;T, E&gt;</code> for recoverable errors</li>
            <li>Use early returns with guard clauses to reduce nesting</li>
          </ul>

          <CodeBlock
            language="jitzu"
            code={`// Good: Early returns reduce nesting
fun validate_and_process(input: String): Result<String, String> {
    if input == "" {
        return Err("Input cannot be empty")
    }

    // Happy path - main logic
    Ok(input)
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <p>
            Functions are the building blocks of Jitzu programs. Next, explore{" "}
            <a href="/docs/language/control-flow" className="text-primary hover:underline">
              Control Flow
            </a>{" "}
            to learn about conditionals and loops.
          </p>
        </section>
      </ScrollReveal>
    </article>
  );
}
