namespace RepoQuill.Core;

/// <summary>
/// Configuration for a RepoQuill extraction operation.
/// </summary>
public record QuillConfig
{
    /// <summary>
    /// The root directory to scan.
    /// </summary>
    public required string RootPath { get; init; }

    /// <summary>
    /// Glob patterns for files to include with full content.
    /// If empty, all non-binary text files are included by default.
    /// </summary>
    public IReadOnlyList<string> IncludePatterns { get; init; } = [];

    /// <summary>
    /// Glob patterns for files to exclude entirely (not in tree or content).
    /// </summary>
    public IReadOnlyList<string> ExcludePatterns { get; init; } = [];

    /// <summary>
    /// Glob patterns for files to include in tree only (no content).
    /// Binary files are automatically added to tree-only unless explicitly excluded.
    /// </summary>
    public IReadOnlyList<string> TreeOnlyPatterns { get; init; } = [];

    /// <summary>
    /// Whether to honor .gitignore files (including nested ones).
    /// </summary>
    public bool HonorGitIgnore { get; init; } = true;

    /// <summary>
    /// Whether to strip comments from source files.
    /// </summary>
    public bool StripComments { get; init; } = false;

    /// <summary>
    /// Whether to normalize whitespace in source files.
    /// </summary>
    public bool NormalizeWhitespace { get; init; } = false;

    /// <summary>
    /// The output format to use.
    /// </summary>
    public OutputFormat Format { get; init; } = OutputFormat.PlainText;
}

/// <summary>
/// The format for the generated output.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Plain text with ASCII tree and file content sections.
    /// </summary>
    PlainText,

    /// <summary>
    /// Structured JSON output.
    /// </summary>
    Json
}
