# Jitzu vs Other Languages: Feature Showcase

Jitzu combines the best features from multiple modern programming languages. Here's how familiar patterns translate to Jitzu's syntax sugar-packed approach.

## Language Inspirations

Jitzu draws inspiration from: **Rust**, **C#**, **F#**, **Go**, **TypeScript**, **Scala**, and **Zig**

---

## Variable Declaration & Mutability

### Rust-inspired Immutability
```rust
// Rust
let x = 5;           // immutable
let mut y = 5;       // mutable
```

```jitzu
// Jitzu
let x = 5            // immutable by default
let mut y = 5        // explicit mutability
```

### Type Inference (TypeScript/Scala-style)
```typescript
// TypeScript
let name = "Alice";     // inferred as string
let age: number = 30;   // explicit type
```

```jitzu
// Jitzu
let name = "Alice"      // inferred as String
let age: Int = 30       // explicit type
```

---

## String Interpolation

### JavaScript/C# Template Strings
```javascript
// JavaScript
const name = "World";
console.log(`Hello, ${name}!`);
```

```csharp
// C#
var name = "World";
Console.WriteLine($"Hello, {name}!");
```

```jitzu
// Jitzu (same syntax, more power)
let name = "World"
print(`Hello, {name}!`)
print(`Math: {2 + 3 * 4}`)
print(`Object: {person.name.toUpperCase()}`)
```

---

## Pattern Matching

### Rust Match Expressions
```rust
// Rust
match result {
    Ok(value) => println!("Success: {}", value),
    Err(error) => println!("Error: {}", error),
}
```

### F# Pattern Matching
```fsharp
// F#
match pet with
| Cat name -> printfn "Cat named %s" name
| Dog (name, age) -> printfn "Dog %s, age %d" name age
| Fish -> printfn "Just a fish"
```

```jitzu
// Jitzu (combines both styles)
match result {
    Ok(value) => print(`Success: {value}`),
    Err(error) => print(`Error: {error}`),
}

match pet {
    Cat(name) => print(`Cat named {name}`),
    Dog(name, age) => print(`Dog {name}, age {age}`),
    Fish => print("Just a fish"),
}
```

---

## Error Handling

### Rust Result Types
```rust
// Rust
fun divide(a: i32, b: i32) -> Result<i32, String> {
    if b == 0 {
        Err("Division by zero".to_string())
    } else {
        Ok(a / b)
    }
}

let result = divide(10, 2)?; // ? operator
```

### Go Error Handling
```go
// Go
func divide(a, b int) (int, error) {
    if b == 0 {
        return 0, errors.New("division by zero")
    }
    return a / b, nil
}

result, err := divide(10, 2)
if err != nil {
    return err
}
```

```jitzu
// Jitzu (Rust-style with extra sugar)
fun divide(a: Int, b: Int): Result<Int, String> {
    if b == 0 {
        Err("Division by zero")
    } else {
        Ok(a / b)
    }
}

let result = try divide(10, 2)  // try operator (like Rust's ?)

// Or with pattern matching
match divide(10, 0) {
    Ok(val) => print(`Result: {val}`),
    Err(msg) => print(`Error: {msg}`),
}
```

---

## Object-Oriented Programming

### C# Classes and Interfaces
```csharp
// C#
public interface IGreetable {
    string GetGreeting();
}

public class Person : IGreetable {
    public string Name { get; set; }

    public string GetGreeting() {
        return $"Hello, I'm {Name}";
    }
}
```

### Rust Structs and Traits
```rust
// Rust
trait Greetable {
    fun get_greeting(&self) -> String;
}

struct Person {
    name: String,
}

impl Greetable for Person {
    fun get_greeting(&self) -> String {
        format!("Hello, I'm {}", self.name)
    }
}
```

```jitzu
// Jitzu (combines the best of both)
trait Greetable {
    fun get_greeting(self): String
}

type Person {
    pub name: String,
}

impl Greetable for Person {
    fun get_greeting(self): String {
        `Hello, I'm {self.name}`  // String interpolation built-in
    }
}

// Usage
let person = Person { name = "Alice" }
print(person.get_greeting())
```

---

## Functional Programming

### Scala Collections
```scala
// Scala
val numbers = List(1, 2, 3, 4, 5)
val doubled = numbers.map(_ * 2)
val evens = numbers.filter(_ % 2 == 0)
```

### F# Pipe Operator
```fsharp
// F#
let result =
    data
    |> parseJson
    |> validate
    |> transform
    |> saveToDb
```

```jitzu
// Jitzu (work in progress features)
let numbers = [1, 2, 3, 4, 5]
let doubled = numbers.map(|x| x * 2)      // Closure syntax
let evens = numbers.filter(|x| x % 2 == 0)

// Pipeline operator (proposed)
let result = data
    |> parse_json
    |> validate
    |> transform
    |> save_to_db
```

---

## Async Programming

### C# Async/Await
```csharp
// C#
public async Task<string> FetchDataAsync(string url) {
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}
```

### Rust Async
```rust
// Rust
async fun fetch_data(url: &str) -> Result<String, reqwest::Error> {
    let response = reqwest::get(url).await?;
    response.text().await
}
```

```jitzu
// Jitzu (similar to both, with built-in HTTP)
async fun fetch_data(url: String): Result<String, Error> {
    let response = await Http.get(url)
    match response {
        Ok(content) => Ok(content),
        Err(error) => Err(error),
    }
}

// Or simplified with try operator
async fun fetch_data_simple(url: String): Result<String, Error> {
    Ok(try await Http.get(url))
}
```

---

## Memory Management

### Go Garbage Collection
```go
// Go - Garbage collected, no manual memory management
func processData() {
    data := make([]int, 1000)
    // Memory automatically cleaned up
}
```

### Rust Ownership
```rust
// Rust - Manual memory management with ownership
fun process_data() {
    let data = vec![0; 1000];
    // Memory freed when `data` goes out of scope
}
```

```jitzu
// Jitzu - Automatic memory management like Go, but with Drop trait like Rust
fun process_data() {
    let data = [0; 1000]
    // Memory automatically managed
}

// With explicit cleanup using Drop trait
type ResourceWrapper {
    resource: SomeResource,
}

impl Drop for ResourceWrapper {
    fun drop(self) {
        print("Cleaning up resource")
        self.resource.cleanup()
    }
}
```

---

## HTTP and JSON (Built-in vs Libraries)

### Go HTTP
```go
// Go
resp, err := http.Get("https://api.example.com/data")
if err != nil {
    return err
}
defer resp.Body.Close()
body, err := ioutil.ReadAll(resp.Body)
```

### TypeScript/JavaScript Fetch
```typescript
// TypeScript
const response = await fetch('https://api.example.com/data');
const data = await response.json();
```

```jitzu
// Jitzu - HTTP and JSON built into the language
let response = try Http.get("https://api.example.com/data")
let data = Json.parse(response)

// Or in a match expression
match Http.get("https://api.example.com/data") {
    Ok(content) => {
        let parsed = Json.parse(content)
        process(parsed)
    },
    Err(error) => print(`Request failed: {error}`)
}
```

---

## Range Syntax

### Python Ranges
```python
# Python
for i in range(1, 6):      # 1,2,3,4,5
    print(i)

for i in range(1, 5):      # 1,2,3,4
    print(i)
```

### Rust Ranges
```rust
// Rust
for i in 1..=5 {           // 1,2,3,4,5 (inclusive)
    println!("{}", i);
}

for i in 1..5 {            // 1,2,3,4 (exclusive)
    println!("{}", i);
}
```

```jitzu
// Jitzu (same as Rust, plus character ranges)
for i in 1..=5 {           // 1,2,3,4,5 (inclusive)
    print(i)
}

for i in 1..5 {            // 1,2,3,4 (exclusive)
    print(i)
}

for c in 'a'..='z' {       // Character ranges!
    print(c)
}
```

---

## Union Types vs Enums

### TypeScript Union Types
```typescript
// TypeScript
type Pet = "cat" | "dog" | "fish";
type Result<T, E> = { ok: true, value: T } | { ok: false, error: E };
```

### Rust Enums
```rust
// Rust
enum Pet {
    Cat(String),
    Dog(String, u32),
    Fish,
}

enum Result<T, E> {
    Ok(T),
    Err(E),
}
```

```jitzu
// Jitzu (Rust-style enums with union keyword)
union Pet {
    Cat(String),
    Dog(String, Int),
    Fish,
}

union Result<T, E> {
    Ok(T),
    Err(E),
}

// Pattern matching works the same as Rust
match my_pet {
    Cat(name) => print(`Cat: {name}`),
    Dog(name, age) => print(`Dog: {name}, {age} years old`),
    Fish => print("Glub glub"),
}
```

---

## Interactive Shell (REPL)

### Python REPL
```python
>>> 1 + 1
2
>>> name = "Python"
>>> f"Hello {name}!"
'Hello Python!'
```

### Node.js REPL
```javascript
> 1 + 1
2
> const name = "JavaScript"
> `Hello ${name}!`
'Hello JavaScript!'
```

```bash
# Jitzu Shell - PowerShell replacement with full language support
~ 1 + 1
2

~ let name = "Jitzu"
~ `Hello {name}!`
"Hello Jitzu!"

~ Json.parse(`{"hello": "world"}`)
{ hello: "world" }

~ Http.get("https://api.github.com/zen")
Ok("Design for failure.")
```

---

## What Makes Jitzu Special?

### 🍯 **Syntax Sugar Everywhere**
- String interpolation with `{expressions}`
- Range syntax for numbers AND characters
- Pattern matching with guards and destructuring
- Everything-is-an-expression philosophy

### 🔧 **Built-in Batteries**
- HTTP requests without external libraries
- JSON parsing/serialization built-in
- File system operations standardized
- Interactive shell mode for rapid prototyping

### 🎯 **Best of Both Worlds**
- **Memory safety** like Rust, but **automatic management** like Go
- **Strong typing** like TypeScript, but **inference** like Scala
- **Pattern matching** like F#, but **familiar syntax** like Rust
- **Functional features** when you want them, **imperative** when you don't

### ✨ **Ninja Philosophy**
- **"Fast Enough™"** - Optimized for developer productivity
- **"Overengineered"** - Rich feature set for expressive code
- **"Unoriginal"** - Takes proven ideas from successful languages
- **Full of syntax sugar** - Makes common tasks delightful

---

## Migration Examples

### From TypeScript
```typescript
// TypeScript
interface User {
    name: string;
    age: number;
}

const user: User = { name: "Alice", age: 30 };
console.log(`User: ${user.name}, Age: ${user.age}`);
```

```jitzu
// Jitzu
type User {
    pub name: String,
    pub age: Int,
}

let user = User { name = "Alice", age = 30 }
print(`User: {user.name}, Age: {user.age}`)
```

### From Python
```python
# Python
def greet(name="World"):
    return f"Hello, {name}!"

users = ["Alice", "Bob", "Charlie"]
for user in users:
    print(greet(user))
```

```jitzu
// Jitzu
fun greet(name: String = "World"): String {
    `Hello, {name}!`
}

let users = ["Alice", "Bob", "Charlie"]
for user in users {
    print(greet(user))
}
```

### From Rust
```rust
// Rust
fun process_file(path: &str) -> Result<String, std::io::Error> {
    std::fs::read_to_string(path)
}

match process_file("data.txt") {
    Ok(content) => println!("File content: {}", content),
    Err(e) => println!("Error: {}", e),
}
```

```jitzu
// Jitzu (simpler file operations)
fun process_file(path: String): Result<String, Error> {
    File.open(path).read_text()
}

match process_file("data.txt") {
    Ok(content) => print(`File content: {content}`),
    Err(e) => print(`Error: {e}`),
}
```

Jitzu combines familiar patterns with enhanced ergonomics, making it feel natural for developers coming from any of these languages while providing its own unique blend of features and syntax sugar! ✨
