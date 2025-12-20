using RepoQuill.Core.Models;

namespace RepoQuill.Core.Tree;

/// <summary>
/// Renders a tree view of files.
/// </summary>
public interface ITreeRenderer
{
    /// <summary>
    /// Renders a tree view of the given files.
    /// </summary>
    /// <param name="files">The files to include in the tree (Full and TreeOnly states).</param>
    /// <param name="rootPath">The root path (used for display purposes).</param>
    /// <returns>The rendered tree as a string.</returns>
    public string Render(IReadOnlyList<FileEntry> files, string rootPath);
}
