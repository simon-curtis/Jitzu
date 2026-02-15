import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { ScrollReveal } from "@/components/scroll-reveal";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";

export const Route = createFileRoute("/docs/language/basic-syntax")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Basic Syntax</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          This page covers the fundamental syntax elements of Jitzu, including comments, variables, and the import system.
        </p>
      </div>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Comments</Heading2>
          <p>Jitzu supports both single-line and multi-line comments:</p>
          <CodeBlock
            language="jitzu"
            code={`// Single-line comment
print("Hello") // End-of-line comment

/*
 * Multi-line comment
 * Can span multiple lines
 */`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Variables</Heading2>

          <Heading3>Immutable Variables (Default)</Heading3>
          <p>By default, variables in Jitzu are declared with <code>let</code>:</p>
          <CodeBlock
            language="jitzu"
            code={`let x = 42
let name = "Alice"
let is_valid = true`}
          />

          <Heading3>Mutable Variables</Heading3>
          <p>Use the <code>mut</code> keyword to create mutable variables:</p>
          <CodeBlock
            language="jitzu"
            code={`let mut counter = 0
counter += 1
counter = counter * 2

let mut message = "Hello"
message = message + " World"
print(message) // "Hello World"`}
          />

          <Heading3>Type Annotations</Heading3>
          <p>While Jitzu has type inference, you can explicitly annotate types:</p>
          <CodeBlock
            language="jitzu"
            code={`// Type inferred
let age = 25                    // Int
let height = 5.9               // Double
let name = "Bob"               // String

// Explicit types
let age: Int = 25
let height: Double = 5.9
let name: String = "Bob"
let numbers: Int[] = [1, 2, 3]`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Variable Naming</Heading2>
          <p>Jitzu follows these naming conventions:</p>
          <CodeBlock
            language="jitzu"
            code={`// Variables and functions: snake_case
let user_name = "alice"
let max_retry_count = 3

// Types: PascalCase
type UserProfile { ... }`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Scope and Shadowing</Heading2>

          <Heading3>Block Scope</Heading3>
          <p>Variables are scoped to their containing block:</p>
          <CodeBlock
            language="jitzu"
            code={`let x = 1

if true {
    let y = 2
    print(x) // 1 - can access outer scope
    print(y) // 2
}

// print(y) // Error: y is not in scope`}
          />

          <Heading3>Variable Shadowing</Heading3>
          <p>You can shadow variables by declaring new ones with the same name:</p>
          <CodeBlock
            language="jitzu"
            code={`let x = 5
print(x) // 5

let x = "hello" // Shadows the previous x
print(x) // "hello"

{
    let x = true // Shadows again within this block
    print(x) // true
}

print(x) // "hello" - back to the string version`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Import System</Heading2>
          <p>Jitzu uses <code>open</code> statements for imports.</p>

          <Heading3>Basic Imports</Heading3>
          <p>Import from relative paths:</p>
          <CodeBlock
            language="jitzu"
            code={`// Import entire module
open "./utils.jz"

// Import from parent directory
open "../shared_code/helpers.jz"

// Import from subdirectory
open "./math/calculations.jz"`}
          />

          <Heading3>Selective Imports</Heading3>
          <p>Import specific functions or types:</p>
          <CodeBlock
            language="jitzu"
            code={`// Import specific items
open "../shared_code/greet.jz" as { Greet }

// Import multiple items
open "./math.jz" as { add, subtract, multiply }

// Mix of functions and types
open "./models.jz" as { User, create_user, validate_user }`}
          />

          <Heading3>Aliased Imports</Heading3>
          <p>Import with custom names to avoid conflicts:</p>
          <CodeBlock
            language="jitzu"
            code={`// Import entire module with alias
open "./math.jz" as Math

// Use aliased import
let result = Math.add(5, 3)`}
          />

          <Heading3>Import Example</Heading3>
          <p>Here's how you might structure imports in a Jitzu file:</p>
          <CodeBlock
            language="jitzu"
            code={`// Local utilities
open "./utils/string_helpers.jz" as { capitalize }
open "./config.jz" as Config

// Models and types
open "./models/user.jz" as { User, UserRole }

// Business logic
open "./services/auth.jz" as Auth

// Now use the imported functionality
let user = User {
    name = "Alice",
    role = UserRole.Admin
}

if Auth.is_authorized(user) {
    print("Access granted")
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>String Literals</Heading2>
          <p>Jitzu supports multiple string literal formats:</p>
          <CodeBlock
            language="jitzu"
            code={`// Regular strings
let simple = "Hello World"
let with_escape = "Hello\\nWorld\\tTab"

// Template literals (string interpolation)
let name = "Alice"
let age = 30
let message = \`Hello {name}, you are {age} years old\`

// You can use expressions in interpolation
let calculation = \`2 + 2 = {2 + 2}\``}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Semicolons</Heading2>
          <p>Semicolons are optional in Jitzu:</p>
          <CodeBlock
            language="jitzu"
            code={`// These are equivalent
let x = 42;
let x = 42

// Useful for multiple statements on one line
let a = 1; let b = 2; let c = a + b`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Expression vs Statement</Heading2>
          <p>In Jitzu, almost everything is an expression that returns a value:</p>
          <CodeBlock
            language="jitzu"
            code={`// if is an expression
let message = if age >= 18 { "adult" } else { "minor" }

// Blocks are expressions
let result = {
    let x = 10
    let y = 20
    x + y // This value is returned from the block
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <Heading2>Best Practices</Heading2>

          <ul className="list-disc pl-6 space-y-2">
            <li>Prefer <code>let</code> over <code>let mut</code> when possible</li>
            <li>Use descriptive names: <code>user_count</code> instead of <code>uc</code></li>
            <li>Group related variable declarations together</li>
            <li>Group imports logically at the top of files</li>
            <li>Use selective imports to be explicit about dependencies</li>
          </ul>

          <CodeBlock
            language="jitzu"
            code={`// Recommended file organization:

// 1. Imports first
open "./config.jz" as Config
open "./utils.jz" as { helper_function }

// 2. Main logic
let mut attempt_count = 0
let success = false

while attempt_count < 3 && !success {
    success = try_operation()
    attempt_count += 1
}`}
          />
        </section>
      </ScrollReveal>

      <ScrollReveal>
        <section className="space-y-4">
          <p>
            This covers the essential syntax elements you'll use in every Jitzu program. Next, explore{" "}
            <a href="/docs/language/data-types" className="text-primary hover:underline">
              Data Types
            </a>{" "}
            to learn about Jitzu's type system.
          </p>
        </section>
      </ScrollReveal>
    </article>
  );
}
