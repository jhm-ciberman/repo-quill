# AGENTS.md

## Project Overview

RepoQuill is a desktop + CLI tool that extracts selected files from a project/repository and produces a consolidated, prompt-friendly text output. Built in C# using .NET 9, it serves both human users (via an Avalonia GUI) and AI agents/automations (via a CLI).

## Development Commands

### Workflow

The following workflow must be followed when making changes:

1. Make changes.
2. **Format code**: `dotnet format`
3. **Build the entire solution**: `dotnet build`
4. **Run tests**: `dotnet test` (when changes are made in RepoQuill.Core)

Warnings should be treated as errors and fixed.

## Architecture Overview

The project uses a layered architecture with strict dependency flow:

1. **RepoQuill.Core** - Pure .NET library containing all processing logic (no UI dependencies)
2. **RepoQuill.Cli** - Command-line interface built on top of the core library
3. **RepoQuill.Gui** - Avalonia-based cross-platform GUI application

It's crucial that lower layers do not depend on higher layers. `RepoQuill.Core` must not reference anything in `RepoQuill.Cli` or `RepoQuill.Gui`. This is enforced at project level.

### Key Architectural Principles

- **RepoQuill.Core** is completely independent of UI - it's pure processing logic
- **RepoQuill.Cli** and **RepoQuill.Gui** both depend on Core
- CLI and GUI should never depend on each other

### Style Requirements (enforced by .editorconfig)

- **PascalCase** for methods and properties
- **_camelCase** with underscore prefix for private fields
- **Braces on their own line** (all braces)
- **Explicit `this`** is mandatory for properties and methods: `this.Foo = bar;`
- **File-scoped namespaces** required
- **4 spaces** for indentation
- **CRLF** line endings (Windows primary)

### Project Configuration

- **Target Framework**: .NET 9
- **C# Language Version**: Latest (12.0+)
- **Nullable Reference Types**: Enabled
- **GUI Framework**: Avalonia UI
- **CLI Framework**: System.CommandLine

## Event Guidelines

Events should follow .NET conventions:
- Use `EventHandler` or `EventHandler<TEventArgs>` delegates
- Event names should be in the past tense (e.g., `FileDiscovered`, `ProcessingCompleted`)
- Always provide `sender` and `EventArgs` parameters
- Handler methods should use underscore pattern: `Source_EventName` (e.g., `Engine_ProgressChanged`)
- Unsubscribe from events in dispose/cleanup methods to avoid memory leaks

## Documentation Guidelines

- Documentation is required for all public and protected members in **RepoQuill.Core**. This is mandatory.
- Private/internal members should have documentation only where the intent is not obvious.
- Use comments only for non-obvious code. Explain the why, not the what.
- NEVER leak implementation details in public documentation. Keep it high level and focused on the API contract.
- Use `Gets or sets...` for properties, `Gets...` for getters.
- Use `Initializes a new instance of the <see cref="ClassName"/> class.` for constructors.
- Use `Occurs when...` for events.
- Things in **RepoQuill.Core** MUST be documented. CLI and GUI layers can be documented as needed.
- Private methods should NOT be documented unless the implementation is extremely cryptic.
- Methods that override or implement interfaces can use `<inheritdoc />`.

## Commenting Guidelines

- NEVER add "junior dev" comments that explain obvious code.
- NEVER explain WHAT the code is doing. Explain WHY if necessary.
- If you need to explain a WHAT, consider refactoring the code to make it clearer.
- Exceptions can be made for complex algorithms or non-obvious logic, but use sparingly.
