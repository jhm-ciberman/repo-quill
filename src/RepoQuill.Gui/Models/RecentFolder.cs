using System;

namespace RepoQuill.Gui.Models;

/// <summary>
/// Represents a recently opened folder.
/// </summary>
/// <param name="Path">The absolute path to the folder.</param>
/// <param name="LastOpened">When the folder was last opened.</param>
public record RecentFolder(string Path, DateTime LastOpened);
