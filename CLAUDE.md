# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Quick Reference

**Build**
```bash
dotnet build
```

**Run a Jitzu script**
```bash
cd Jitzu.Shell && dotnet run -- ../Tests/namespace_simple_builtin.jz
```

**Run with debug output**
```bash
cd Jitzu.Shell && dotnet run -- -d ../Tests/script.jz
```

**Run tests**
```bash
dotnet test
```

**Clean build artifacts**
```bash
dotnet clean
```

## Repository Structure

**Root Level**
- `Jitzu.Core/` - Language foundation (Lexer, Parser, AST types, bytecode definition)
- `Jitzu.Shell/` - Unified binary: interpreter, shell, CLI entry point
- `Jitzu.Tests/` - Unit test suite
- `Tests/` - Integration test scripts (`.jz` files)
- `site/` - Documentation website
- `extensions/` - VS Code extension

**Solution File**: `Jitzu.sln` (requires .NET 10.0)

## Architecture Overview

Jitzu follows a classic three-phase interpreter architecture:

### Phase 1: Front-End (Language Definition)
- **Location**: `Jitzu.Core/`
- **Components**:
  - `Lexer.cs` - Converts source text into tokens (keywords, identifiers, operators, literals)
  - `Parser.cs` - Parses token stream into Abstract Syntax Tree (AST)
  - `Language/Expressions.cs` - All 40+ AST expression node types
  - `Language/Token.cs` - Token type definitions
  - `Runtime/OpCode.cs` - Bytecode instruction set specification

**Pipeline**: Source Text → Lexer → Tokens → Parser → AST (Expression[])

### Phase 2: Middle-End (Analysis & Optimization)
- **Location**: `Jitzu.Core/Runtime/Compilation/`
- **Components**:
  - `ProgramBuilder.cs` - Type registration, slot mapping, global setup
  - `SemanticAnalyser.cs` - Two-pass type resolution and validation
  - `AstTransformer.cs` - Local variable optimization using slot indices
  - `UserTypeEmitter.cs` - Dynamic type creation via Reflection.Emit (multi-phase to support forward references)

**Pipeline**: AST → Type Resolution → Local Optimization → Semantic Validation

### Phase 3: Back-End (Code Generation & Execution)
- **Location**: `Jitzu.Core/`
- **Components**:
  - `ByteCodeCompiler.cs` - Converts AST to bytecode instructions
  - `ByteCodeInterpreter.cs` - Stack-based VM that executes bytecode
  - `Runtime/ProgramStack.cs` - Execution stack with local frame and global slots
  - `Runtime/RuntimeProgram.cs` - Compiled program metadata (types, functions, slots)

**Pipeline**: AST → Bytecode Compilation → Stack VM Execution → Result

### Full Execution Flow
```
Source Code (.jz)
    ↓ Lexer
Token Stream
    ↓ Parser
Abstract Syntax Tree
    ↓ ProgramBuilder (type registration)
RuntimeProgram (types + globals + functions)
    ↓ SemanticAnalyser (2-pass type resolution)
Typed AST
    ↓ AstTransformer (local optimization)
Optimized AST with slot indices
    ↓ ByteCodeCompiler
Bytecode (OpCode instructions + constants)
    ↓ ByteCodeInterpreter
Result
```

See `Jitzu.Shell/Program.cs` for the actual orchestration.

## Type System

### Type Storage

Types are stored in three layers for namespace support:

1. **`RuntimeProgram.Types`** - All registered types (key: simple or fully qualified name, value: CLR Type)
2. **`RuntimeProgram.SimpleTypeCache`** - Fast lookup for unambiguous names (built during ProgramBuilder initialization)
3. **`RuntimeProgram.TypeNameConflicts`** - Tracks ambiguous names (name → set of fully qualified names)

**Type Registration Sources**:
- **Built-in types**: Registered in `ProgramBuilder.Build()` (lines 29-47)
  - Primitives: `Int`, `String`, `Bool`, `Double`, `Char`
  - Special types: `Date`, `Time`, `DateTime`
  - Union types: `Result<,>`, `Option<>`, `Ok<>`, `Err<>`, `Some<>`, `None`
- **NuGet types**: Loaded via `PackageResolver.ResolveAsync()` with full namespace preservation
- **User types**: Created via `UserTypeEmitter.RegisterUserTypes()` using Reflection.Emit

### Type Resolution

**Type resolution happens in two places**:

1. **SemanticAnalyser** (`ResolveType()` method, lines 401-450)
   - Called during semantic analysis to resolve type names to CLR types
   - Checks `SimpleTypeCache` first for unambiguous matches
   - Falls back to `TypeNameConflicts` when ambiguous
   - Supports qualified names via dot notation (e.g., `System.Collections.List`)

2. **AstTransformer** (lines 87-91)
   - Checks `SimpleTypeCache` for type identifiers at transform time
   - Transforms type references into `GlobalGetExpression` for runtime access

### Forward References in User Types

**Problem**: User type A referencing user type B before B is defined in the same file.

**Solution**: Multi-phase type registration in `UserTypeEmitter.RegisterUserTypes()`:
1. Phase 1 - Extract all type names
2. Phase 2 - Create `UserTypeDescriptor` objects with empty fields
3. Phase 3 - Create `TypeBuilder` objects (allows forward references in Phase 4)
4. Phase 4 - Resolve field types using extended type dictionary with TypeBuilders
5. Phase 5 - Create actual types and register in `RuntimeProgram.Types`

See `UserTypeEmitter.cs:11-119` for implementation.

## Compilation Pipeline Details

### ProgramBuilder.Build()

**Responsibilities**:
1. Initialize type dictionary with built-in types
2. Load and register NuGet assemblies via `PackageResolver`
3. Build type resolution caches (`SimpleTypeCache`, `TypeNameConflicts`)
4. Create `SlotMap` for global variables using `SlotMapBuilder`
5. Register global functions (`print`, `rand`)
6. Call `UserTypeEmitter.RegisterUserTypes()` for user-defined types
7. Return `RuntimeProgram` containing all compiled metadata

**Output**: `RuntimeProgram` record with:
- `Types` - All available types
- `SimpleTypeCache` & `TypeNameConflicts` - Namespace support
- `Globals` - Global variable types
- `GlobalFunctions` - Global function registry
- `SlotMap` - Global slot indices
- `MethodTable` - Type method registry

### SemanticAnalyser.AnalyseScript()

**Two-Pass Strategy**:

**Pass 1** - Function header registration:
- Extract return types from `FunctionDefinitionExpression`
- Register in `program.GlobalFunctions`

**Pass 2** - Full recursive analysis:
- **Identifiers** → Create `GlobalGetExpression` or `LocalGetExpression` with cached type
- **Type references** → Resolve via `ResolveType()` (checks SimpleTypeCache/TypeNameConflicts)
- **Function calls** → Cache function reference and return type
- **Binary operations** → Resolve operand types
- **If/Match expressions** → Type check conditions and branches
- **Indexing** → Always returns `Option<ElementType>` (safe indexing)
- **Global assignment** → Create `GlobalSetExpression` with type tracking

**Result**: Typed AST ready for code generation

### AstTransformer.TransformExpression()

**Purpose**: Optimize local variable access using slot indices

**Key Transformations**:
- `IdentifierLiteral` → `LocalGetExpression` (if local variable) or cached type reference
- `LetExpression` → Push scope, allocate slot, emit `LocalSetExpression`
- `SimpleMemberAccessExpression` → Check if qualified type name, otherwise member access

**Slot Management**: Uses `SlotMapBuilder` to assign unique indices per variable (0-based within frame)

**Result**: AST with local variable access optimized for fast runtime lookup

### ByteCodeCompiler.Compile()

**Process**: Recursive depth-first AST traversal, emitting bytecode instructions

**Constant Pooling**: Literals deduplicated in `Chunk.Constants` dictionary

**Label Patching**: Jump targets (if/while) patched in second pass

**Example Compilation**:
```jz
let x = 5
x + 1
```
Becomes bytecode:
```
LoadConst(constIdx=0)      // Push 5
SetLocal(slotIdx=0)         // Store in x
GetLocal(slotIdx=0)         // Load x
LoadConst(constIdx=1)      // Push 1
Add                         // x + 1
```

## Runtime Execution Model

### Stack-Based VM (ByteCodeInterpreter)

**Execution State**:
- **ProgramStack** - Main execution stack (Value array up to 256 elements)
- **StackPointer** - Current stack position (-1 = empty)
- **FrameBase** - Offset to current function's local frame
- **CallFrames** - Array tracking nested function calls
- **InstructionPointer (IP)** - Bytecode position in current function

### Value Type (Tagged Union)

```csharp
struct Value {
    byte Kind;  // Null | Int | Double | Bool | Ref
    int I32;    // For integers
    double F64; // For floating-point
    bool B;     // For booleans
    object Ref; // For objects/references
}
```

**8 bytes total**, enables type-safe stack operations without type metadata overhead.

### Opcode Categories

**Stack Operations**: `Dup`, `Pop`, `Swap`, `LoadConst`

**Local/Global Access**: `GetLocal`, `SetLocal`, `GetGlobal`, `SetGlobal`

**Object Operations**: `Construct`, `GetField`, `SetField`, `IndexGet`, `IndexSet`, `NewArray`, `NewString`

**Arithmetic**: `Add`, `Sub`, `Mul`, `Div`, `Mod`, `Inc`, `Dec`

**Comparison**: `Lt`, `Lte`, `Gt`, `Gte`, `Eq`, `Compare`

**Control Flow**: `Jump`, `JumpIfFalse`, `Loop`, `Call`, `Return`

**Union Types**: `TryUnwrap`, `UnwrapUnion` (for Result/Option pattern matching)

See `Jitzu.Core/Runtime/OpCode.cs` for complete instruction set.

### Function Calls

```
OpCode.Call(argCount)
```

Execution:
1. Stack contains: `[... | function | arg0 | arg1 | ... | argN]`
2. Pop function and arguments
3. Create new `CallFrame` with current IP
4. Switch to function's bytecode and locals
5. Local frame reserved for parameters + locals
6. When `Return` encountered, restore previous IP and stack state

### Local Variable Slots

**Frame Layout**:
```
Stack [... | FrameBase | param0 | param1 | local0 | local1 | ... | StackPointer]
                      ↑
                   Frame starts here
```

**Access**: `GetLocal(slotIdx)` → `Stack[FrameBase + slotIdx]`

**Scope Management**: `SlotMapBuilder` tracks lexical scopes and reuses slots for non-overlapping scopes

## Important Implementation Details

### Namespace Support (Recent Implementation)

**File-based namespacing for user types**: `UserTypeEmitter.DeriveNamespaceFromFilePath()` derives namespace from file path relative to project root.

**Full CLR namespace preservation for NuGet types**: `ProgramBuilder.Build()` line 62 uses `type.FullName ?? type.Name` to preserve full namespaces.

**Type cache building**: `ProgramBuilder.BuildTypeResolutionCaches()` (lines 163-202) populates:
- `SimpleTypeCache` - names with single match
- `TypeNameConflicts` - names with multiple matches

**Error reporting**: Ambiguous type references suggest fully qualified names:
```
Error: Type 'JsonSerializer' is ambiguous. Did you mean:
  - System.Text.Json.JsonSerializer
  - Newtonsoft.Json.JsonSerializer
```

### Global Type Resolution

**ByteCodeInterpreter initialization** (lines 48-56):
```csharp
foreach (var (name, index) in program.SlotMap)
{
    if (program.GlobalFunctions.TryGetValue(name, out var function))
        _programStack.SetGlobal(index, Value.FromRef(function));
    else if (program.Types.TryGetValue(name, out var type))
        _programStack.SetGlobal(index, Value.FromRef(type));
    else if (program.SimpleTypeCache.TryGetValue(name, out type))
        _programStack.SetGlobal(index, Value.FromRef(type));
}
```

Falls back to `SimpleTypeCache` for unambiguous type lookups.

### Reflection.Emit Usage (UserTypeEmitter)

**Dynamic type creation**:
- `DynamicTypeFactory` uses `ModuleBuilder` to define types at runtime
- Properties created as auto-properties with backing fields
- Default constructor generated
- `TypeBuilder` objects usable as types before `CreateType()` (enables forward references)

See `UserTypeEmitter.cs:204-301` for DynamicTypeFactory implementation.

## Common Development Tasks

### Adding a New Built-in Type

1. Add CLR type to `ProgramBuilder.Build()` dictionary (line 29+)
2. Example: `["MyType"] = typeof(MyClass)`
3. Type automatically available in type cache

### Debugging Script Execution

Use `-d` flag to dump stack at each instruction:
```bash
dotnet run -- -d script.jz
```

Output shows:
- Current instruction pointer (IP)
- Stack contents with Value types
- Source location from debug spans

### Testing New Compiler Changes

1. Add test script to `Tests/` directory
2. Run: `dotnet run -- ../Tests/test.jz`
3. Verify output matches expected

### Understanding Type Resolution Failures

If "Unknown type identifier: TypeName" error occurs:

1. Check `ProgramBuilder` - is type registered? (lines 29-65)
2. Check `SimpleTypeCache` - is name ambiguous? (check `TypeNameConflicts`)
3. Try fully qualified name in code (e.g., `System.Collections.List` instead of `List`)
4. For user types, ensure they're defined before use (unless same file - multi-phase handles it)

### Adding a New Opcode

1. Add variant to `OpCode` enum in `Jitzu.Core/Runtime/OpCode.cs`
2. Add emission logic to `ByteCodeCompiler.CompileExpression()` or helper
3. Add execution handler to `ByteCodeInterpreter.Evaluate()` switch statement
4. Test with compilation + execution

## Key Files Reference

**Language Definition**:
- `Jitzu.Core/Lexer.cs` - Tokenization
- `Jitzu.Core/Parser.cs` - Parsing (ref struct, recursive descent)
- `Jitzu.Core/Language/Expressions.cs` - 40+ AST node types
- `Jitzu.Core/Language/Token.cs` - Token definitions

**Compilation**:
- `Jitzu.Core/Runtime/Compilation/ProgramBuilder.cs` - Setup & type registration
- `Jitzu.Core/Runtime/Compilation/SemanticAnalyser.cs` - Type resolution (2-pass)
- `Jitzu.Core/Runtime/Compilation/AstTransformer.cs` - Local optimization
- `Jitzu.Core/Runtime/Compilation/ByteCodeCompiler.cs` - Code generation

**Runtime**:
- `Jitzu.Core/ByteCodeInterpreter.cs` - Stack VM execution
- `Jitzu.Core/Runtime/RuntimeProgram.cs` - Compiled program metadata
- `Jitzu.Core/Runtime/ProgramStack.cs` - Execution stack
- `Jitzu.Core/Runtime/OpCode.cs` - Bytecode instruction set

**Execution**:
- `Jitzu.Shell/Program.cs` - CLI entry point & orchestration

**Type System**:
- `Jitzu.Core/Runtime/UserTypeDescriptor.cs` - User type metadata
- `Jitzu.Core/Runtime/Compilation/UserTypeEmitter.cs` - Dynamic type creation
- `Jitzu.Core/Runtime/TypeRegistry.cs` - Type registration helper

## Namespace Architecture

The namespace implementation follows a three-level cache strategy:

1. **Primary Storage** (`Types`): Full qualified names → CLR Type
2. **Fast Cache** (`SimpleTypeCache`): Simple names → Type (when unambiguous)
3. **Conflict Map** (`TypeNameConflicts`): Simple names → {set of full names} (when ambiguous)

**Resolution Priority**:
1. Check `SimpleTypeCache` for fast path
2. If not found, check `TypeNameConflicts` for ambiguity error
3. As fallback, check full `Types` dictionary for qualified names

**File Namespace Derivation**:
- File path: `project/models/user.jz` → Namespace: `models.User`
- Relative to project root (determined at compilation time)
- Stored in `RuntimeProgram.FileNamespaces` for multi-file support

This architecture maintains backward compatibility (simple names still work) while supporting arbitrary NuGet imports without collisions.

## Performance Considerations

**Stack-based VM**: No registers, simple instruction dispatch (switch/case) on OpCode

**Constant pooling**: Literals deduped in `Chunk.Constants`, reduces memory

**Local slot optimization**: Direct stack frame access via indices (no dictionary lookups)

**Value tagging**: 8-byte Value struct fits in registers, minimal GC pressure for primitives

**Stack allocation**: Parser is ref struct for zero allocation during tokenization

**Two-pass compilation**: Allows label patching without jump table overhead
