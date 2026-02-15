import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/language/control-flow")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Control Flow</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          Jitzu provides expressive control flow constructs including conditionals, loops, and early returns.
          Many control flow constructs are expressions, meaning they return values.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Conditionals</Heading2>

        <Heading3>Basic if Statements</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Simple if statement
if 2 > 1 {
    print("2 is greater than 1")
}`}
        />

        <Heading3>if-else Chains</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// if-else chain
if 2 < 1 {
    print("This won't print")
} else if 1 > 1 {
    print("Nor this")
} else {
    print("But this will")
}`}
        />

        <Heading3>if Expressions</Heading3>
        <p>Since <code>if</code> is an expression, it returns a value:</p>
        <CodeBlock
          language="jitzu"
          code={`// if expressions (return values)
let x = 10
let result = if x > 0 { "positive" } else { "non-positive" }
print(result) // "positive"

// Inline conditional
let message = if logged_in { "Welcome back!" } else { "Please log in" }`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Loops</Heading2>

        <Heading3>Range Loops</Heading3>
        <p>Jitzu provides convenient range-based loops:</p>
        <CodeBlock
          language="jitzu"
          code={`// Inclusive range (1 to 5)
for i in 1..=5 {
    print(\` > {i}\`)
}
// Output: 1, 2, 3, 4, 5

// Exclusive range (1 to 4)
for i in 1..5 {
    print(\` > {i}\`)
}
// Output: 1, 2, 3, 4

// Character ranges
for c in 'a'..='z' {
    print(\` > {c}\`)
}
// Output: a, b, c, ..., z`}
        />

        <Heading3>Collection Iteration</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Iterate over vector elements
let numbers = [1, 2, 3, 4, 5]
for num in numbers {
    print(\`Number: {num}\`)
}`}
        />

        <Heading3>While Loops</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Basic while loop
let mut i = 0
while i < 10 {
    print(\`Count: {i}\`)
    i += 1
}

// While with complex condition
let mut attempts = 0
let mut success = false

while attempts < 3 && !success {
    success = try_operation()
    attempts += 1
}

// Infinite loop with break
let mut counter = 0
while true {
    counter += 1
    if counter > 100 {
        break
    }
}`}
        />

        <Heading3>Loop Control</Heading3>
        <p>Control loop execution with <code>break</code> and <code>continue</code>:</p>
        <CodeBlock
          language="jitzu"
          code={`// Using break and continue
for i in 1..=100 {
    if i == 50 {
        break // Exit the loop completely
    }

    if i % 2 == 0 {
        continue // Skip to next iteration
    }

    print(i) // Only prints odd numbers up to 49
}`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Early Returns</Heading2>
        <CodeBlock
          language="jitzu"
          code={`// Early return from function
fun find_first_even(numbers: Int[]): Option<Int> {
    for num in numbers {
        if num % 2 == 0 {
            return Some(num)
        }
    }
    None // No even number found
}

// Guard clauses
fun process_user(user: User): Result<String, String> {
    if !user.is_active {
        return Err("User is not active")
    }

    if user.age < 18 {
        return Err("User must be 18 or older")
    }

    // Main processing logic here
    Ok(\`Processing user: {user.name}\`)
}`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Best Practices</Heading2>

        <ul className="list-disc pl-6 space-y-2">
          <li>Use guard clauses to handle edge cases first</li>
          <li>Reduce nesting by returning early</li>
          <li>Use <code>for</code> loops for known iterations</li>
          <li>Use <code>while</code> loops for condition-based iteration</li>
          <li>Combine conditions rather than deeply nesting</li>
        </ul>

        <CodeBlock
          language="jitzu"
          code={`// Avoid deep nesting
// Bad:
if condition1 {
    if condition2 {
        if condition3 {
            // deeply nested code
        }
    }
}

// Better: Combine conditions
if condition1 && condition2 && condition3 {
    // main logic here
}`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <p>
          Control flow is fundamental to programming logic in Jitzu. Next, explore{" "}
          <a href="/docs/language/object-oriented" className="text-primary hover:underline">
            Types
          </a>{" "}
          to learn about custom type definitions and composition.
        </p>
      </section>
      </ScrollReveal>
    </article>
  );
}
