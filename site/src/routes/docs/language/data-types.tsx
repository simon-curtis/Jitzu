import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { ScrollReveal } from "@/components/scroll-reveal";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";

export const Route = createFileRoute("/docs/language/data-types")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Data Types</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          Jitzu provides a set of built-in data types with strong typing and type inference
          to catch errors early while keeping code concise.
        </p>
      </div>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Numbers</Heading2>

          <Heading3>Integer Types</Heading3>
          <CodeBlock
            language="jitzu"
            code={`// Integers (default: Int)
let x = 20
let y = 10
let negative = -42

// Basic arithmetic operations
print(x + y)  // Addition: 30
print(x - y)  // Subtraction: 10
print(x * y)  // Multiplication: 200
print(x / y)  // Division: 2
print(x % y)  // Modulus: 0`}
          />

          <Heading3>Floating-Point Numbers</Heading3>
          <CodeBlock
            language="jitzu"
            code={`// Double precision by default
let pi = 3.14159
let radius = 5.0
let area = pi * radius * radius

// Scientific notation
let large_number = 1.23e6  // 1,230,000
let small_number = 4.56e-3 // 0.00456`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Strings</Heading2>

          <Heading3>String Literals</Heading3>
          <CodeBlock
            language="jitzu"
            code={`// Simple strings
print("Hello World")
print("Hello\\nWorld")    // With escape sequences
print("Hello\\tWorld")    // Tab character`}
          />

          <Heading3>String Interpolation</Heading3>
          <p>One of Jitzu's most convenient features is template literal interpolation:</p>
          <CodeBlock
            language="jitzu"
            code={`// Basic interpolation
let greeting = "Hello"
let name = "Simon"
print(\`{greeting}, {name}!\`)  // "Hello, Simon!"

// Expression interpolation
print(\`1 + 1 = {1 + 1}\`)      // "1 + 1 = 2"

// Complex expressions
let count = 5
print(\`There are {count} items\`)
print(\`Math result: {count * 2 + 1}\`)`}
          />

          <Heading3>String Concatenation</Heading3>
          <CodeBlock
            language="jitzu"
            code={`let greeting = "Hello"
let full_message = greeting + " World"
print(full_message)  // "Hello World"

// String comparison
let name1 = "Alice"
let name2 = "alice"
print(name1 == name2)  // false (case-sensitive)`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Vectors (Arrays)</Heading2>
          <p>Vectors are dynamic arrays that can grow at runtime.</p>

          <Heading3>Creating Vectors</Heading3>
          <CodeBlock
            language="jitzu"
            code={`// Create empty typed vector
let strings = String[]
strings.push("Hello")
strings.push("World")
print(strings)  // ["Hello", "World"]

// Initialize with values
let numbers = [1, 2, 3, 4, 5]
let names = ["Alice", "Bob", "Charlie"]

// Explicit typing
let ages: Int[] = [25, 30, 35]
let scores: Double[] = [95.5, 87.2, 92.8]`}
          />

          <Heading3>Vector Operations</Heading3>
          <CodeBlock
            language="jitzu"
            code={`let numbers = [1, 2, 3]

// Adding elements
numbers.push(4)
numbers.push(5)

// Accessing elements
print(numbers[0])     // First element: 1
print(numbers[-1])    // Last element: 5

// Vector length
print(numbers.length) // 5

// Iteration
for item in numbers {
    print(\`Number: {item}\`)
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Dynamic Objects</Heading2>
          <p>Jitzu supports dynamic object creation for flexible data structures:</p>

          <CodeBlock
            language="jitzu"
            code={`// Simple object
let person = {
    name = "John",
    age = 30,
    email = "john@example.com"
}

// Nested objects
let user = {
    profile = {
        name = "Alice",
        avatar = "avatar.png"
    },
    settings = {
        theme = "dark",
        notifications = true
    }
}

// Accessing fields with dot notation
print(person.name)        // "John"
print(user.profile.name)  // "Alice"`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Boolean Type</Heading2>
          <CodeBlock
            language="jitzu"
            code={`let is_active = true
let is_complete = false

// Boolean operations
let can_proceed = is_active && !is_complete
let should_stop = !is_active || is_complete

// Boolean from comparisons
let is_adult = age >= 18`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Option Types</Heading2>
          <p>Option types handle nullable values safely:</p>
          <CodeBlock
            language="jitzu"
            code={`// Option types
fun find_user(id: Int): Option<User> {
    if id > 0 {
        Some(User { id = id, name = "Found" })
    } else {
        None
    }
}

match find_user(123) {
    Some(user) => print(\`Found: {user.name}\`),
    None => print("User not found")
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <p>
            This covers Jitzu's core data types. Next, explore{" "}
            <a href="/docs/language/functions" className="text-primary hover:underline">
              Functions
            </a>{" "}
            to learn how to define and use functions.
          </p>
        </section>
      </ScrollReveal>
    </article>
  );
}
