namespace RepoQuill.Gui.Models;

/// <summary>
/// Represents the UI state of a file or folder node in the tree.
/// </summary>
public enum FileNodeState
{
    /// <summary>
    /// Full content will be included (checkbox checked).
    /// </summary>
    Checked,

    /// <summary>
    /// Excluded from output (checkbox unchecked).
    /// </summary>
    Unchecked,

    /// <summary>
    /// Only included in tree structure, no content (icon shows [T]).
    /// </summary>
    TreeOnly,

    /// <summary>
    /// Folder with mixed children states.
    /// </summary>
    Indeterminate
}
