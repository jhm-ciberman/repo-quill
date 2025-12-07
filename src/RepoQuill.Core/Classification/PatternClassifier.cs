using RepoQuill.Core.Models;
using RepoQuill.Core.Utilities;

namespace RepoQuill.Core.Classification;

/// <summary>
/// Classifies files based on glob patterns and binary detection.
/// </summary>
public sealed class PatternClassifier : IFileClassifier
{
    /// <inheritdoc/>
    public FileEntry Classify(FileEntry entry, QuillConfig config)
    {
        var relativePath = entry.RelativePath;

        // 1. Check exclude patterns first (highest priority)
        if (config.ExcludePatterns.Count > 0 &&
            GlobMatcher.MatchesAny(relativePath, config.ExcludePatterns))
        {
            return entry.WithState(FileState.Excluded);
        }

        // 2. Check tree-only patterns
        if (config.TreeOnlyPatterns.Count > 0 &&
            GlobMatcher.MatchesAny(relativePath, config.TreeOnlyPatterns))
        {
            return entry.WithState(FileState.TreeOnly);
        }

        // 3. Check if binary (auto tree-only)
        if (BinaryDetector.IsBinary(entry.AbsolutePath))
        {
            return entry.WithState(FileState.TreeOnly);
        }

        // 4. Check include patterns (if specified)
        if (config.IncludePatterns.Count > 0)
        {
            // If include patterns are specified, file must match to be included
            if (GlobMatcher.MatchesAny(relativePath, config.IncludePatterns))
            {
                return entry.WithState(FileState.Full);
            }
            else
            {
                // Doesn't match any include pattern - exclude
                return entry.WithState(FileState.Excluded);
            }
        }

        // 5. Default: include as full (text file, no restrictions)
        return entry.WithState(FileState.Full);
    }
}
