namespace RepoQuill.Core.Models;

/// <summary>
/// Represents a file entry with its loaded content.
/// </summary>
/// <param name="Entry">The file entry metadata.</param>
/// <param name="Content">The text content of the file.</param>
public record FileContent(FileEntry Entry, string Content);
