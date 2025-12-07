using System.Runtime.CompilerServices;
using RepoQuill.Core.Models;

namespace RepoQuill.Core.Discovery;

/// <summary>
/// Scans directories for files, respecting .gitignore rules.
/// </summary>
public sealed class FileScanner : IFileDiscovery
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<FileEntry> DiscoverAsync(
        string rootPath,
        bool honorGitIgnore,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        rootPath = Path.GetFullPath(rootPath);

        if (!Directory.Exists(rootPath))
            yield break;

        // Build ignore rules from all .gitignore files
        var ignoreRules = honorGitIgnore
            ? await BuildIgnoreRulesAsync(rootPath, ct)
            : null;

        await foreach (var entry in ScanDirectoryAsync(rootPath, rootPath, ignoreRules, ct))
        {
            yield return entry;
        }
    }

    /// <summary>
    /// Recursively scans a directory for files.
    /// </summary>
    private async IAsyncEnumerable<FileEntry> ScanDirectoryAsync(
        string rootPath,
        string currentPath,
        Ignore.Ignore? ignoreRules,
        [EnumeratorCancellation] CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        // Process files in current directory
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(currentPath);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }

        foreach (var filePath in files)
        {
            ct.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);

            // Skip hidden files (starting with .)
            if (fileName.StartsWith('.'))
                continue;

            var relativePath = GetRelativePath(rootPath, filePath);

            // Check if file is ignored
            if (ignoreRules != null && IsIgnored(ignoreRules, relativePath))
                continue;

            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(filePath);
            }
            catch
            {
                continue;
            }

            yield return new FileEntry
            {
                AbsolutePath = filePath,
                RelativePath = relativePath,
                SizeBytes = fileInfo.Length,
                LastModified = fileInfo.LastWriteTimeUtc,
                State = FileState.Full // Will be classified later
            };
        }

        // Process subdirectories
        IEnumerable<string> directories;
        try
        {
            directories = Directory.EnumerateDirectories(currentPath);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (DirectoryNotFoundException)
        {
            yield break;
        }

        foreach (var dirPath in directories)
        {
            ct.ThrowIfCancellationRequested();

            var dirName = Path.GetFileName(dirPath);

            // Skip hidden directories (starting with .)
            if (dirName.StartsWith('.'))
                continue;

            var relativeDirPath = GetRelativePath(rootPath, dirPath);

            // Check if directory is ignored
            if (ignoreRules != null && IsIgnored(ignoreRules, relativeDirPath + "/"))
                continue;

            await foreach (var entry in ScanDirectoryAsync(rootPath, dirPath, ignoreRules, ct))
            {
                yield return entry;
            }
        }
    }

    /// <summary>
    /// Builds ignore rules from all .gitignore files in the tree.
    /// </summary>
    private async Task<Ignore.Ignore> BuildIgnoreRulesAsync(string rootPath, CancellationToken ct)
    {
        var ignore = new Ignore.Ignore();

        // Add default ignores
        ignore.Add(".git/");

        // Find all .gitignore files
        var gitignoreFiles = new List<string>();
        CollectGitIgnoreFiles(rootPath, gitignoreFiles);

        // Sort by depth (root first, then nested)
        gitignoreFiles.Sort((a, b) => a.Length.CompareTo(b.Length));

        // Add rules from each file
        foreach (var gitignorePath in gitignoreFiles)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var lines = await File.ReadAllLinesAsync(gitignorePath, ct);
                var gitignoreDir = Path.GetDirectoryName(gitignorePath)!;
                var relativeDir = GetRelativePath(rootPath, gitignoreDir);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    // Skip empty lines and comments
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                        continue;

                    // Prefix with directory path for nested gitignores
                    // relativeDir is "." for root, empty string won't happen
                    var rule = relativeDir == "."
                        ? trimmed
                        : $"{relativeDir}/{trimmed}";

                    ignore.Add(rule);
                }
            }
            catch
            {
                // Skip unreadable .gitignore files
            }
        }

        return ignore;
    }

    /// <summary>
    /// Recursively collects all .gitignore file paths.
    /// </summary>
    private void CollectGitIgnoreFiles(string dirPath, List<string> results)
    {
        var gitignorePath = Path.Combine(dirPath, ".gitignore");
        if (File.Exists(gitignorePath))
        {
            results.Add(gitignorePath);
        }

        try
        {
            foreach (var subDir in Directory.EnumerateDirectories(dirPath))
            {
                var dirName = Path.GetFileName(subDir);
                if (!dirName.StartsWith('.'))
                {
                    CollectGitIgnoreFiles(subDir, results);
                }
            }
        }
        catch
        {
            // Skip inaccessible directories
        }
    }

    /// <summary>
    /// Checks if a path is ignored.
    /// </summary>
    private bool IsIgnored(Ignore.Ignore ignore, string relativePath)
    {
        // Normalize path separators
        relativePath = relativePath.Replace('\\', '/');
        return ignore.IsIgnored(relativePath);
    }

    /// <summary>
    /// Gets the relative path from root to target.
    /// </summary>
    private string GetRelativePath(string rootPath, string targetPath)
    {
        var relative = Path.GetRelativePath(rootPath, targetPath);
        // Normalize to forward slashes
        return relative.Replace('\\', '/');
    }
}
