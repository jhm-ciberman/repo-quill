using RepoQuill.Core.Models;

namespace RepoQuill.Core.Transforms;

/// <summary>
/// Transforms file content.
/// </summary>
public interface IContentTransform
{
    /// <summary>
    /// Transforms the content of a file.
    /// </summary>
    /// <param name="content">The file content to transform.</param>
    /// <returns>A new FileContent with transformed content.</returns>
    FileContent Transform(FileContent content);
}
