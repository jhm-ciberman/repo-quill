using System.Collections.Generic;
using System.Threading.Tasks;
using RepoQuill.Gui.Models;

namespace RepoQuill.Gui.Services;

/// <summary>
/// Service for persisting application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the list of recently opened folders.
    /// </summary>
    public Task<IReadOnlyList<RecentFolder>> GetRecentFoldersAsync();

    /// <summary>
    /// Adds a folder to the recent list (moves to top if already present).
    /// </summary>
    public Task AddRecentFolderAsync(string path);
}
