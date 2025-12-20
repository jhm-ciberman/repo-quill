using RepoQuill.Core.Models;

namespace RepoQuill.Core.Formatting;

/// <summary>
/// Formats the final output from file entries and contents.
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Formats the output.
    /// </summary>
    /// <param name="allFiles">All files (Full and TreeOnly) for the tree section.</param>
    /// <param name="contents">Content of Full files.</param>
    /// <returns>The formatted output string.</returns>
    public string Format(IReadOnlyList<FileEntry> allFiles, IReadOnlyList<FileContent> contents);
}
