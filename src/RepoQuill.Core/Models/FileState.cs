namespace RepoQuill.Core.Models;

/// <summary>
/// Represents how a file should be included in the output.
/// </summary>
public enum FileState
{
    /// <summary>
    /// File appears in tree and content is included.
    /// </summary>
    Full,

    /// <summary>
    /// File appears in tree but content is not included.
    /// </summary>
    TreeOnly,

    /// <summary>
    /// File is completely excluded from output.
    /// </summary>
    Excluded
}
