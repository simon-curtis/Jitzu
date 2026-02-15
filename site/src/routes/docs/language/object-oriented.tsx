import { createFileRoute } from "@tanstack/react-router";
import { CodeBlock } from "@/components/code-block";
import { Heading1, Heading2, Heading3 } from "@/components/ui/heading";
import { ScrollReveal } from "@/components/scroll-reveal";

export const Route = createFileRoute("/docs/language/object-oriented")({
    component: RouteComponent,
});

function RouteComponent() {
    return (
        <article className="space-y-8 text-foreground max-w-4xl">
            <Heading1>Types</Heading1>

            <div className="space-y-4">
                <p className="text-lg text-muted-foreground">
                    Jitzu lets you define custom types to group related data together.
                    Types support public and private fields, nesting, and composition.
                </p>
            </div>

            <ScrollReveal>
            <section className="space-y-4">
                <Heading2>Type Definitions</Heading2>

                <Heading3>Basic Types</Heading3>
                <p>Define custom types to group related data:</p>
                <CodeBlock
                    language="jitzu"
                    code={`// Basic type definition
type Person {
    pub name: String,
    pub age: Int
}

// Creating instances
let john = Person {
    name = "John Doe",
    age = 30
}

// Accessing fields
print(john.name)  // "John Doe"
print(john.age)   // 30`}
                />

                <Heading3>Nested Types</Heading3>
                <p>Types can contain other types for complex data structures:</p>
                <CodeBlock
                    language="jitzu"
                    code={`// Nested type definitions
type PersonName {
    pub first: String,
    pub last: String
}

type Address {
    pub street: String,
    pub city: String,
    pub zip_code: String
}

type Employee {
    pub name: PersonName,
    pub id: Int,
    pub department: String,
    pub address: Address
}

// Creating nested instances
let employee = Employee {
    name = PersonName {
        first = "Alice",
        last = "Johnson"
    },
    id = 12345,
    department = "Engineering",
    address = Address {
        street = "123 Main St",
        city = "Tech City",
        zip_code = "12345"
    }
}

// Accessing nested fields
print(\`Employee: {employee.name.first} {employee.name.last}\`)
print(\`Works in: {employee.department}\`)`}
                />

                <Heading3>Private and Public Fields</Heading3>
                <p>Control field visibility with <code>pub</code>. Fields without <code>pub</code> are private:</p>
                <CodeBlock
                    language="jitzu"
                    code={`type BankAccount {
    pub account_number: String,
    pub owner: String,
    balance: Double,  // Private field
    pin: String       // Private field
}`}
                />
            </section>
            </ScrollReveal>

            <ScrollReveal>
            <section className="space-y-4">
                <Heading2>Composition</Heading2>

                <p>Jitzu favors composition - build complex types by combining simpler ones:</p>
                <CodeBlock
                    language="jitzu"
                    code={`// Base components
type Engine {
    pub horsepower: Int,
    pub fuel_type: String
}

type Transmission {
    pub gear_type: String,
    pub gears: Int
}

// Composed type
type Car {
    pub make: String,
    pub model: String,
    pub engine: Engine,
    pub transmission: Transmission
}

let my_car = Car {
    make = "Toyota",
    model = "Supra",
    engine = Engine {
        horsepower = 382,
        fuel_type = "Gasoline"
    },
    transmission = Transmission {
        gear_type = "manual",
        gears = 6
    }
}

print(\`{my_car.make} {my_car.model} - {my_car.engine.horsepower}hp\`)`}
                />
            </section>
            </ScrollReveal>

            <ScrollReveal>
            <section className="space-y-4">
                <Heading2>Best Practices</Heading2>

                <ul className="list-disc pl-6 space-y-2">
                    <li><strong>Small, focused types</strong> - Keep types simple and focused on one responsibility</li>
                    <li><strong>Use composition</strong> - Build complex types from simpler ones rather than making one huge type</li>
                    <li><strong>Public interface design</strong> - Only mark fields as <code>pub</code> when they need to be accessed externally</li>
                    <li><strong>Descriptive names</strong> - Use PascalCase for type names and snake_case for field names</li>
                </ul>
            </section>
            </ScrollReveal>

            <ScrollReveal>
            <section className="space-y-4">
                <p>
                    Next, explore{" "}
                    <a href="/docs/language/pattern-matching" className="text-primary hover:underline">
                        Pattern Matching
                    </a>{" "}
                    to learn about union types and match expressions.
                </p>
            </section>
            </ScrollReveal>
        </article>
    );
}
