# Jitzu Quick Reference

A fast reference guide for the Jitzu programming language syntax and features.

## Variables & Constants

```jitzu
let x = 42                    // Immutable (default)
let mut counter = 0           // Mutable variable
const PI = 3.14159           // Compile-time constant
```

## Data Types

### Numbers
```jitzu
let int_num = 42
let float_num = 3.14
let hex = 0xFF
let binary = 0b1010
let parsed = try Int.parse("42")
```

### Strings
```jitzu
"Hello World"                 // Basic string
'Single quotes'               // Also valid
`Hello {name}!`              // String interpolation
`Result: {2 + 3}`            // Expressions in templates
```

### Collections
```jitzu
let array = [1, 2, 3]        // Mixed-type array
let strings = String[]       // Typed array
strings.push("hello")        // Add element
```

### Objects
```jitzu
let person = {
    name = "John",
    age = 30,
    active = true
}
```

## Functions

```jitzu
// Basic function
fun add(a: Int, b: Int): Int {
    a + b
}

// Type inference
fun multiply(x: Int, y: Int) {
    x * y
}

// Recursive
fun factorial(n: Int): Int {
    if n <= 1 { 1 } else { n * factorial(n - 1) }
}
```

## Control Flow

### Conditionals
```jitzu
if condition {
    // code
} else if other {
    // code
} else {
    // code
}

// Expression form
let result = if x > 0 { "positive" } else { "negative" }
```

### Loops
```jitzu
// Range loops
for i in 1..=5 { }           // 1,2,3,4,5 (inclusive)
for i in 1..5 { }            // 1,2,3,4 (exclusive)
for c in 'a'..='z' { }       // Character ranges

// While loop
while condition {
    // code
}

// Collection iteration
for item in collection {
    // code
}
```

## Pattern Matching

### Union Types
```jitzu
union Option<T> {
    Some(T),
    None,
}

union Result<T, E> {
    Ok(T),
    Err(E),
}
```

### Match Expressions
```jitzu
match value {
    Ok(result) => print(`Success: {result}`),
    Err(error) => print(`Error: {error}`),
}

match pet {
    Cat(name) => print(`Cat named {name}`),
    Dog(name, age) => print(`Dog {name}, age {age}`),
    Fish => print("Just a fish"),
}
```

## Types & Traits

### Type Definitions
```jitzu
type Person {
    pub name: String,
    pub age: Int,
}

// Create instance
let john = Person {
    name = "John",
    age = 30
}
```

### Traits
```jitzu
trait Drawable {
    fun draw(self): String
}

impl Drawable for Person {
    fun draw(self): String {
        `Person: {self.name}`
    }
}
```

### Implementation Blocks
```jitzu
impl Person {
    fun new(name: String, age: Int): Person {
        Person { name = name, age = age }
    }

    fun greet(self): String {
        `Hello, I'm {self.name}`
    }
}
```

## Error Handling

### Try Operator
```jitzu
let result = try risky_operation()    // Unwrap or propagate error
let file_content = try File.read("data.txt")
```

### Try-Catch
```jitzu
try {
    let data = risky_operation()
    process(data)
} catch error {
    print(`Error: {error}`)
} finally {
    cleanup()
}
```

### Defer
```jitzu
fun with_resource() {
    let resource = acquire()
    defer resource.cleanup()    // Runs when function exits

    // Use resource...
}
```

## File Operations

```jitzu
// Read file
let content = try File.open("file.txt").read_text()

// Write file
let writer = try File.open("output.txt").writer()
try writer.write_text("Hello, World!")
writer.drop()

// List directory
for file in try Path.read_files(".") {
    print(file)
}
```

## HTTP Requests

```jitzu
// GET request
let response = Http.get("https://api.example.com/data")
match response {
    Ok(data) => process(data),
    Err(error) => handle_error(error),
}

// POST request
let payload = { name = "John", age = 30 }
let response = Http.post("https://api.example.com/users", payload)
```

## JSON

```jitzu
// Parse JSON
let data = Json.parse(`{"name": "John", "age": 30}`)

// Serialize to JSON
let person = Person { name = "John", age = 30 }
let json_text = Json.text(person)
```

## Imports

```jitzu
open "./utils.jz"                    // Import file
open "./math.jz" as Math            // With alias
open "../shared/greet.jz" as { Greet }  // Specific imports
```

## Operators

### Arithmetic
```jitzu
+  -  *  /  %                       // Basic math
++ --                               // Increment/decrement
+= -= *= /= %=                      // Compound assignment
```

### Comparison
```jitzu
== != < > <= >=                     // Comparison
is                                  // Type checking
like                                // Pattern matching
```

### Logical
```jitzu
and or not                          // Logical operators
&& ||                               // Short-circuit
```

### Other
```jitzu
..  ..=                             // Ranges
=>                                  // Match arms
->                                  // Function types
??                                  // Null coalescing
?.                                  // Optional chaining (WIP)
```

## Comments

```jitzu
// Single line comment
/* Multi-line
   comment */
print("Hello") // End of line comment
```

## Special Features

### Everything is an Expression
```jitzu
let result = {
    let x = 10
    let y = 20
    x + y  // Last expression is returned
}
```

### String Interpolation
```jitzu
let name = "World"
let greeting = `Hello, {name}!`
let math = `2 + 2 = {2 + 2}`
let complex = `User: {user.name.toUpperCase()}`
```

### Range Syntax
```jitzu
1..5        // 1,2,3,4 (exclusive end)
1..=5       // 1,2,3,4,5 (inclusive end)
'a'..'z'    // Character ranges
```

## Shell Mode

```bash
~ 1 + 1
2

~ let x = 42
~ x * 2
84

~ Json.parse(`{"hello": "world"}`)
{ hello: "world" }
```

## Best Practices

- Use `let` by default, `let mut` only when needed
- Prefer pattern matching over multiple `if` statements
- Use `try` operator for error propagation
- Leverage string interpolation for readable output
- Use traits for shared behavior
- Implement `Drop` trait for cleanup logic
- Use `defer` for resource cleanup

## Common Patterns

```jitzu
// Option handling
match maybe_value {
    Some(val) => process(val),
    None => handle_empty(),
}

// Result chaining
let final_result = try step1()
    |> step2
    |> step3;

// Builder pattern
let config = {
    host = "localhost",
    port = 8080,
    ssl = true
}
```
