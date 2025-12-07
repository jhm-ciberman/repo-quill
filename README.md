# RepoQuill

> **Note**: This project is currently under active development.

**RepoQuill** is a desktop + CLI tool that extracts selected files from a project/repository and produces a consolidated, prompt-friendly text output. The tool serves both human users (via a GUI) and AI agents/automations (via a CLI).

## Overview

RepoQuill helps you prepare code for AI assistants by:
- Selecting files from your repository
- Applying filters and transformations
- Generating consolidated, prompt-friendly output
- Supporting both interactive (GUI) and automated (CLI) workflows

## Architecture

RepoQuill uses a layered architecture with strict separation of concerns:

1. **RepoQuill.Core** - Pure .NET library containing all processing logic
2. **RepoQuill.Cli** - Command-line interface built on top of the core library
3. **RepoQuill.Gui** - Avalonia-based cross-platform GUI application

## Features

- **File Discovery**: Intelligent file traversal with `.gitignore` support
- **Filtering**: Include/exclude patterns, file size limits
- **Transformations**: Comment stripping, whitespace normalization
- **Multiple Output Formats**: Plain text concatenation or structured JSON
- **Cross-Platform**: Windows (primary), Linux/macOS support

## Tech Stack

- **Runtime**: .NET 9
- **Language**: C# 12.0
- **GUI Framework**: Avalonia UI
- **CLI Framework**: System.CommandLine

## Project Structure

```
repo-quill/
├── src/
│   ├── RepoQuill.Core/       # Core processing logic
│   ├── RepoQuill.Cli/         # Command-line interface
│   └── RepoQuill.Gui/         # Avalonia GUI application
├── tests/
│   └── RepoQuill.Core.Tests/  # Unit tests for core library
├── RepoQuill.sln              # Solution file
├── Directory.Build.props      # Common MSBuild properties
├── .editorconfig              # Code style configuration
└── README.md                  # This file
```

## Building

```bash
# Build the entire solution
dotnet build RepoQuill.sln

# Run the CLI
dotnet run --project src/RepoQuill.Cli/RepoQuill.Cli.csproj

# Run the GUI
dotnet run --project src/RepoQuill.Gui/RepoQuill.Gui.csproj

# Run tests
dotnet test
```

## Development

This project follows strict coding standards enforced by `.editorconfig`:

- **Nullable Reference Types**: Enabled
- **File-Scoped Namespaces**: Required
- **Braces**: On their own line
- **Naming Conventions**:
  - PascalCase for public members
  - _camelCase for private fields
- **Documentation**: Required for all public members in Core library

## License

MIT License - see LICENSE file for details.
