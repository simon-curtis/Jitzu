import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/language/pattern-matching")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <article className="space-y-8 text-foreground max-w-4xl">
      <Heading1>Pattern Matching</Heading1>

      <div className="space-y-4">
        <p className="text-lg text-muted-foreground">
          Pattern matching is one of Jitzu's most powerful features. Combined with union types and the
          built-in Result/Option system, it lets you handle different cases elegantly and safely.
        </p>
      </div>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Union Types</Heading2>
        <p>Union types allow a value to be one of several variants, providing type-safe alternatives to traditional enums.</p>

        <Heading3>Defining Union Types</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Basic union type
union Pet {
    Fish,
    Cat(String),           // Cat with name
    Dog(String, Int),      // Dog with name and age
    Bird(String, Bool),    // Bird with name and can_talk
    None,
}

// Union for error handling
union FileResult {
    Success(String),
    NotFound,
    PermissionDenied,
    InvalidFormat(String),
}`}
        />

        <Heading3>Creating Union Instances</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Creating union instances
let my_pet = Pet.Cat("Whiskers")
let family_dog = Pet.Dog("Rex", 5)
let goldfish = Pet.Fish
let no_pet = Pet.None

// Result instances
let success = FileResult.Success("File content here")
let error = FileResult.InvalidFormat("Not a valid JSON file")`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Match Expressions</Heading2>
        <p>Match expressions provide exhaustive pattern matching over union types.</p>

        <Heading3>Basic Pattern Matching</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Basic match expression
let pet = Pet.Cat("Whiskers")

match pet {
    Pet.Fish => print("Fish don't need names"),
    Pet.Cat(name) => print(\`Hello cat {name}\`),
    Pet.Dog(name, age) => print(\`Dog {name} is {age} years old\`),
    Pet.Bird(name, can_talk) => {
        if can_talk {
            print(\`{name} the talking bird\`)
        } else {
            print(\`{name} the quiet bird\`)
        }
    },
    Pet.None => print("No pets"),
}

// Match expressions return values
let pet_description = match pet {
    Pet.Fish => "A silent swimmer",
    Pet.Cat(name) => \`A cat named {name}\`,
    Pet.Dog(name, age) => \`A {age}-year-old dog named {name}\`,
    Pet.Bird(name, _) => \`A bird named {name}\`,
    Pet.None => "No pet"
}`}
        />

        <Heading3>Wildcard Patterns</Heading3>
        <p>Use <code>_</code> to ignore values you don't need:</p>
        <CodeBlock
          language="jitzu"
          code={`match pet {
    Pet.Dog(name, _) => print(\`Dog: {name}\`),  // Ignore age
    _ => print("Not a dog"),                      // Match anything else
}`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Result Types</Heading2>
        <p>Result types are a built-in union for handling operations that can succeed or fail.</p>

        <Heading3>Working with Results</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Function returning Result
fun divide(a: Double, b: Double): Result<Double, String> {
    if b == 0.0 {
        Err("Division by zero")
    } else {
        Ok(a / b)
    }
}

// Pattern matching Results
let result = divide(10.0, 2.0)

match result {
    Ok(value) => print(\`Result: {value}\`),
    Err(error) => print(\`Error: {error}\`)
}`}
        />

        <Heading3>The Try Operator</Heading3>
        <p>Use <code>try</code> to propagate errors without nested match statements:</p>
        <CodeBlock
          language="jitzu"
          code={`fun safe_sqrt(x: Double): Result<Double, String> {
    if x < 0.0 {
        Err("Cannot take square root of negative number")
    } else {
        Ok(x)
    }
}

// Using try for early return on errors
fun complex_calculation(a: Double, b: Double, c: Double): Result<Double, String> {
    let step1 = try divide(a, b)        // Returns Err early if division fails
    let step2 = try safe_sqrt(step1)    // Returns Err early if sqrt fails
    let step3 = try divide(step2, c)    // Returns Err early if division fails
    Ok(step3)
}

// Without try (more verbose)
fun complex_calculation_verbose(a: Double, b: Double, c: Double): Result<Double, String> {
    match divide(a, b) {
        Ok(step1) => {
            match safe_sqrt(step1) {
                Ok(step2) => divide(step2, c),
                Err(e) => Err(e)
            }
        },
        Err(e) => Err(e)
    }
}`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Option Types</Heading2>
        <p>Option types handle nullable values safely.</p>

        <Heading3>Option Patterns</Heading3>
        <CodeBlock
          language="jitzu"
          code={`// Function returning Option
fun find_user(users: User[], id: Int): Option<User> {
    for user in users {
        if user.id == id {
            return Some(user)
        }
    }
    None
}

// Pattern matching Options
match find_user(users, 1) {
    Some(user) => print(\`Found user: {user.name}\`),
    None => print("User not found")
}`}
        />
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <Heading2>Best Practices</Heading2>

        <ul className="list-disc pl-6 space-y-2">
          <li><strong>Cover all cases</strong> - Match expressions should be exhaustive</li>
          <li><strong>Use wildcard patterns</strong> - Use <code>_</code> for cases you don't care about</li>
          <li><strong>Prefer Result/Option</strong> - Use them instead of null checks for safer code</li>
          <li><strong>Use try</strong> - The try operator keeps error propagation clean and readable</li>
        </ul>
      </section>
      </ScrollReveal>

      <ScrollReveal>
      <section className="space-y-4">
        <p>
          Pattern matching makes Jitzu code more expressive and safer by ensuring all cases are handled.
          It's particularly powerful when combined with union types and the Result/Option system. Next, explore the{" "}
          <a href="/docs/shell/overview" className="text-primary hover:underline">
            Jitzu Shell
          </a>{" "}
          to learn about the interactive shell environment.
        </p>
      </section>
      </ScrollReveal>
    </article>
  );
}
