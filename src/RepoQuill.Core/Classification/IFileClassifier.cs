using RepoQuill.Core.Models;

namespace RepoQuill.Core.Classification;

/// <summary>
/// Classifies files into Full, TreeOnly, or Excluded states.
/// </summary>
public interface IFileClassifier
{
    /// <summary>
    /// Classifies a file entry based on configuration rules.
    /// </summary>
    /// <param name="entry">The file entry to classify.</param>
    /// <param name="config">The configuration with patterns.</param>
    /// <returns>A new FileEntry with the appropriate State set.</returns>
    public FileEntry Classify(FileEntry entry, QuillConfig config);
}
