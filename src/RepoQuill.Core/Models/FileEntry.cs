namespace RepoQuill.Core.Models;

/// <summary>
/// Represents a discovered file with its metadata and classification state.
/// </summary>
public record FileEntry
{
    /// <summary>
    /// Gets the absolute path to the file.
    /// </summary>
    public required string AbsolutePath { get; init; }

    /// <summary>
    /// Gets the path relative to the root directory.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// Gets the last modification time of the file.
    /// </summary>
    public required DateTime LastModified { get; init; }

    /// <summary>
    /// Gets how this file should be included in the output.
    /// </summary>
    public FileState State { get; init; } = FileState.Full;

    /// <summary>
    /// Creates a copy of this entry with a different state.
    /// </summary>
    /// <param name="state">The new file state.</param>
    /// <returns>A new <see cref="FileEntry"/> with the specified state.</returns>
    public FileEntry WithState(FileState state) => this with { State = state };
}
