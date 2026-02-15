# Jitzu Language Guide

A modern scripting language designed to be "Fast Enough™, Overengineered, and Unoriginal" - packed with syntax sugar to make scripting fun and expressive.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Basic Syntax](#basic-syntax)
3. [Data Types](#data-types)
4. [Functions](#functions)
5. [Control Flow](#control-flow)
6. [Object-Oriented Programming](#object-oriented-programming)
7. [Pattern Matching](#pattern-matching)
8. [File Operations](#file-operations)
9. [HTTP Requests](#http-requests)
10. [Advanced Features](#advanced-features)
11. [Shell Mode](#shell-mode)

---

## Getting Started

Jitzu is a modern scripting language that combines features from Rust, C#, F#, Go, TypeScript, Scala, and Zig. It's designed for both script execution and interactive shell usage.

### Basic Program Structure

```jitzu
// Simple Hello World
print("Hello, World!")

// With string interpolation
let name = "Jitzu"
print(`Hello from {name}!`)
```

---

## Basic Syntax

### Comments

```jitzu
// Single-line comment
print("Hello") // End-of-line comment

/*
 * Multi-line comment
 * Can span multiple lines
 */
```

### Variables and Constants

```jitzu
// Immutable by default
let x = 42
let name = "Alice"

// Mutable variables
let mut counter = 0
counter += 1

// Constants (compile-time)
const PI = 3.14159
```

### Import System

```jitzu
// Import from relative path
open "./utils.jz"

// Import specific functions
open "../shared_code/greet.jz" as { Greet }

// Import with aliasing
open "./math.jz" as Math
```

---

## Data Types

### Numbers

Jitzu supports various numeric types with automatic type inference:

```jitzu
// Integers
let x = 20
let y = 10

// Basic arithmetic
print(x + y) // Addition: 30
print(x - y) // Subtraction: 10
print(x * y) // Multiplication: 200
print(x / y) // Division: 2
print(x % y) // Modulus: 0

// Combined expressions
print(x + y - x * y / x % y)

// Parsing from strings
let parsed_int = try Int.parse("42")
let parsed_double = try Double.parse("42.42")
```

### Strings

Jitzu provides powerful string handling with interpolation:

```jitzu
// Simple strings
print("Hello World")
print("Hello\nWorld")    // With escape sequences
print("Hello\tWorld")    // Tab character

// String concatenation
let greeting = "Hello"
let name = greeting + " World"

// String interpolation (template literals)
print(`{greeting}, Simon`)
print(`1 + 1 = {1 + 1}`)

// Complex interpolation
let count = 5
print(`There are {count} items`)
print(`Math result: {count * 2 + 1}`)
```

### Vectors (Arrays)

```jitzu
// Typed vector
let strings = String[] // or: let strings: String[] = []
strings.push("Hello")
print(strings)

// Mixed-type vector (variant)
let things = [1, 'a', false]
things.push("mixed types!")
print(things)

// Vector operations
let numbers = [1, 2, 3, 4, 5]
numbers.push(6)
```

### Dates and Time

```jitzu
// Date parsing
let date1 = try Date.parse("2024-01-01")
let date2 = try Date.parse("2024-01-01")
print(date1 == date2) // true

// Time operations
let date3 = try Date.parse("2024-01-02")
let time = try Time.parse("12:00:00")
let timestamp = date3 + time

// Timestamp comparison
let timestamp2 = try Timestamp.parse("2024-01-02 12:00:00")
print(timestamp == timestamp2)
```

### Dynamic Objects

```jitzu
// Dynamic object creation
let person = {
    name = "John",
    age = 30,
    car = {
        name = "Ford",
        model = "Mustang"
    }
}

// Object comparison (deep equality)
let person2 = {
    name = "John",
    age = 30,
    car = {
        name = "Ford",
        model = "Mustang"
    }
}

print(person == person2) // true
```

---

## Functions

### Function Definition

```jitzu
// Basic function
fun add(a: Int, b: Int): Int {
    a + b
}

// Recursive functions
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

// Using functions
print(add(5, 3))        // 8
print(factorial(5))     // 120
print(power(2, 8))      // 256
```

### Function Features

- **Everything is an Expression**: Functions automatically return the last expression
- **Type Inference**: Return types can often be inferred
- **First-class Functions**: Functions can be assigned to variables and passed around

---

## Control Flow

### Conditionals

```jitzu
// Simple if statement
if 2 > 1 {
    print("2 is greater than 1")
}

// if-else chain
if 2 < 1 {
    print("This won't print")
} else if 1 > 1 {
    print("Nor this")
} else {
    print("But this will")
}

// if expressions (return values)
let result = if x > 0 { "positive" } else { "non-positive" }
```

### Loops

```jitzu
// Range loops (inclusive)
for i in 1..=5 {
    print(` > {i}`)
}

// Range loops (exclusive)
for i in 1..5 {
    print(` > {i}`)
}

// Character ranges
for c in 'a'..='z' {
    print(` > {c}`)
}

// While loops
let mut i = 0
while i < 10 {
    print(`Count: {i}`)
    i += 1
}

// Loop control
for i in 1..=100 {
    if i == 50 {
        break
    }
    if i % 2 == 0 {
        continue
    }
    print(i)
}
```

---

## Object-Oriented Programming

### Type Definitions

```jitzu
// Basic type
type Person {
    pub name: String,
    pub age: Int
}

// Nested types
type PersonName {
    pub first: String,
    pub last: String
}

type Employee {
    pub name: PersonName,
    pub id: Int,
    pub department: String
}

// Creating instances
let john = Person {
    name = "John Doe",
    age = 30
}
```

### Traits (Interfaces)

```jitzu
// Define a trait
trait Greetable {
    fun get_greeting(self): String
}

// Implement trait for type
impl Greetable for Person {
    fun get_greeting(self): String {
        `Hello, my name is {self.name}!`
    }
}

// Special traits
trait Drop {
    fun drop(self)
}

impl Drop for Person {
    fun drop(self) {
        print(`Dropping person: {self.name}`)
    }
}
```

### Implementation Blocks

```jitzu
// Add methods to types
impl Person {
    fun new(name: String, age: Int): Person {
        Person { name = name, age = age }
    }

    fun birthday(self) {
        self.age += 1
        print(`Happy birthday! Now {self.age}`)
    }
}

// Using methods
let person = Person.new("Alice", 25)
person.birthday()
```

---

## Pattern Matching

### Union Types

```jitzu
// Define union type
union Pet {
    Fish,
    Cat(String),
    Dog(String, Int), // name, age
    None,
}

// Create instances
let pet = Pet.Cat("Whiskers")
let dog = Pet.Dog("Rex", 5)
```

### Match Expressions

```jitzu
// Pattern matching
match pet {
    Fish => print("Fish don't need names"),
    Cat(name) => print(`Hello cat {name}`),
    Dog(name, age) => print(`Dog {name} is {age} years old`),
    None => print("No pets"),
}

// Match with conditions
match number {
    x if x > 100 => print("Big number"),
    x if x > 10 => print("Medium number"),
    _ => print("Small number"),
}
```

### Result Types

```jitzu
// Working with Result<T, E>
fun divide(a: Int, b: Int): Result<Int, String> {
    if b == 0 {
        Err("Division by zero")
    } else {
        Ok(a / b)
    }
}

// Pattern match results
match divide(10, 2) {
    Ok(result) => print(`Result: {result}`),
    Err(error) => print(`Error: {error}`)
}

// Try operator
let result = try divide(10, 2) // Unwraps Ok, propagates Err
```

---

## File Operations

### Reading and Writing Files

```jitzu
// Open file for writing
let output_file = File.open("output.txt")
try output_file.delete() // Delete if exists

// Get writer
let writer = try output_file.writer()

// Read files from directory
for file_path in try Path.read_files(".") {
    if file_path == output_file.full_name {
        continue // Skip output file
    }

    print(`Processing {file_path}`)

    let file = File.open(file_path)
    let content = try file.read_text()
    try writer.write_text(content)
}

// Clean up (implements Drop trait)
writer.drop()
```

### File System Operations

```jitzu
// File operations
let file = File.open("data.txt")
let exists = file.exists()
let content = try file.read_text()

// Directory operations
let files = try Path.read_files("./documents")
for file in files {
    print(`Found: {file}`)
}
```

---

## HTTP Requests

### Making HTTP Requests

```jitzu
// GET request
let response = Http.get("https://api.example.com/users/123")

match response {
    Ok(content) => {
        print("Success!")
        print(content)
    },
    Err(error) => print(`Request failed: {error}`)
}

// POST request with JSON
let user_data = { name = "John", age = 30 }
let post_response = Http.post("https://api.example.com/users", user_data)

// Using in expressions
let api_result = try Http.get("https://api.github.com/users/octocat")
let user = Json.parse(api_result)
print(`User: {user.name}`)
```

---

## Advanced Features

### JSON Handling

```jitzu
// Define structured types
type Attack {
    pub name: String,
    pub damage: Int,
}

type Person {
    pub name: String,
    pub attacks: Attack[],
    pub level: Int,
}

// Create object
let warrior = Person {
    name = "Conan",
    attacks = [
        Attack { name = "Sword Strike", damage = 25 },
        Attack { name = "Shield Bash", damage = 15 }
    ],
    level = 10
}

// Serialize to JSON
let json_text = Json.text(warrior)
print(`Encoded: {json_text}`)

// Deserialize from JSON
let restored = Json.parse(json_text)
print(`Decoded: {restored.name}`)
```

### Iterator Trait

```jitzu
type Counter {
    mut current: Int = 0,
    max: Int
}

impl Iter for Counter {
    type T = Int

    fun next(self): Bool {
        if self.current >= self.max {
            return false
        }

        self.current += 1
        true
    }
}

// Using iterator
let counter = Counter { current = 0, max = 5 }
while counter.next() {
    print(`Count: {counter.current}`)
}
```

### Error Handling

```jitzu
// Try-catch blocks
try {
    let risky_operation = might_fail()
    print(`Success: {risky_operation}`)
} catch error {
    print(`Caught error: {error}`)
} finally {
    print("Cleanup code here")
}

// Try operator for early returns
fun process_data(): Result<String, String> {
    let data = try load_file("data.txt")
    let parsed = try parse_data(data)
    let processed = try transform(parsed)
    Ok(processed)
}

// Defer for cleanup
fun with_resource() {
    let resource = acquire_resource()
    defer resource.cleanup() // Runs when function exits

    // Use resource...
}
```

---

## Shell Mode

Jitzu includes an interactive shell mode for quick experimentation:

```bash
~ 1 + 1
2

~ let name = "Jitzu"
~ `Hello from {name}!`
"Hello from Jitzu!"

~ Json.parse(`{ "language": "Jitzu" }`)
{ language: "Jitzu" }

~ Http.get("https://api.github.com/zen")
Ok("Design for failure.")

~ for i in 1..=3 { print(`Number {i}`) }
Number 1
Number 2
Number 3
```

### Shell Features

- **Immediate evaluation** of expressions
- **Variable persistence** across commands
- **Pretty printing** of results
- **Error handling** with graceful degradation
- **Multi-line support** for complex expressions

---

## Language Features Summary

### ✨ **Key Features**

- **Immutable by default** - Explicit `mut` for mutable data
- **Strong type system** - With inference and gradual typing
- **Pattern matching** - Powerful match expressions and destructuring
- **Traits system** - Similar to Rust traits or TypeScript interfaces
- **First-class functions** - Functions as values
- **String interpolation** - Template literals with `{expression}` syntax
- **Result types** - Explicit error handling with `Result<T, E>`
- **Everything is an expression** - No statements vs expressions distinction
- **Async support** - Built-in async/await (work in progress)
- **Memory safety** - Automatic memory management
- **Interactive shell** - REPL for quick experimentation

### 🎯 **Design Philosophy**

Jitzu is designed to be:
- **Fast Enough™** - Optimized for developer productivity over raw performance
- **Overengineered** - Includes many language features for expressiveness
- **Unoriginal** - Borrows the best ideas from other languages
- **Fun to use** - Syntax sugar makes common tasks enjoyable

---

## Examples and Patterns

### Common Patterns

```jitzu
// Builder pattern with dynamic objects
let config = {
    host = "localhost",
    port = 8080,
    ssl = true,
    middleware = ["cors", "auth", "logging"]
}

// Functional programming
let numbers = [1, 2, 3, 4, 5]
let doubled = numbers.map(|x| x * 2)
let evens = numbers.filter(|x| x % 2 == 0)

// Pipeline operator (proposed)
let result = data
    |> parse_json
    |> validate
    |> transform
    |> save_to_db

// Option chaining (work in progress)
let user_email = user?.profile?.email ?? "unknown@example.com"
```

This guide covers the core features of Jitzu. The language is actively evolving with new features being added regularly. Check the official documentation and examples for the latest updates!
