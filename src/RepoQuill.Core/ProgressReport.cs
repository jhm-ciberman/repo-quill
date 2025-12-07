namespace RepoQuill.Core;

/// <summary>
/// Reports progress during a RepoQuill operation.
/// </summary>
public record ProgressReport
{
    /// <summary>
    /// The current phase of processing.
    /// </summary>
    public required ProgressPhase Phase { get; init; }

    /// <summary>
    /// The file currently being processed (empty if not applicable).
    /// </summary>
    public required string CurrentFile { get; init; }

    /// <summary>
    /// Number of items processed so far in the current phase.
    /// </summary>
    public required int ProcessedCount { get; init; }

    /// <summary>
    /// Total number of items to process in the current phase.
    /// -1 if the total is unknown (e.g., during discovery).
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Progress percentage (0-100), or -1 if unknown.
    /// </summary>
    public int ProgressPercent => TotalCount > 0
        ? (int)((ProcessedCount / (double)TotalCount) * 100)
        : -1;
}

/// <summary>
/// The phases of a RepoQuill operation.
/// </summary>
public enum ProgressPhase
{
    /// <summary>
    /// Scanning directories and discovering files.
    /// </summary>
    Discovering,

    /// <summary>
    /// Classifying files into Full, TreeOnly, or Excluded.
    /// </summary>
    Classifying,

    /// <summary>
    /// Loading file contents.
    /// </summary>
    Loading,

    /// <summary>
    /// Applying transformations (comment stripping, etc.).
    /// </summary>
    Transforming,

    /// <summary>
    /// Generating the final output.
    /// </summary>
    Formatting
}
