using RepoQuill.Core.Models;

namespace RepoQuill.Core;

/// <summary>
/// The result of a RepoQuill extraction operation.
/// </summary>
public record QuillResult
{
    /// <summary>
    /// Gets the formatted output (plain text or JSON).
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// Gets the total number of files processed (Full + TreeOnly).
    /// </summary>
    public required int TotalFiles { get; init; }

    /// <summary>
    /// Gets the number of files with full content included.
    /// </summary>
    public required int FullFiles { get; init; }

    /// <summary>
    /// Gets the number of files included in tree only.
    /// </summary>
    public required int TreeOnlyFiles { get; init; }

    /// <summary>
    /// Gets the total size in bytes of all processed files.
    /// </summary>
    public required long TotalSizeBytes { get; init; }

    /// <summary>
    /// Gets all files that were processed (Full and TreeOnly, not Excluded).
    /// </summary>
    public required IReadOnlyList<FileEntry> Files { get; init; }

    /// <summary>
    /// Gets the errors encountered during processing (e.g., unreadable files).
    /// </summary>
    public required IReadOnlyList<FileError> Errors { get; init; }
}

/// <summary>
/// Represents an error that occurred while processing a file.
/// </summary>
/// <param name="FilePath">The path of the file that caused the error.</param>
/// <param name="Message">The error message.</param>
public record FileError(string FilePath, string Message);
