using RepoQuill.Core.Models;

namespace RepoQuill.Core.Discovery;

/// <summary>
/// Discovers files in a directory tree.
/// </summary>
public interface IFileDiscovery
{
    /// <summary>
    /// Discovers all files under the given root path.
    /// Files are yielded as they are discovered (streaming).
    /// </summary>
    /// <param name="rootPath">The root directory to scan.</param>
    /// <param name="honorGitIgnore">Whether to respect .gitignore files.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An async enumerable of discovered file entries.</returns>
    public IAsyncEnumerable<FileEntry> DiscoverAsync(
        string rootPath,
        bool honorGitIgnore,
        CancellationToken ct = default);
}
