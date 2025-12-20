using System.Threading.Tasks;
using Avalonia.Controls;

namespace RepoQuill.Gui.Services;

/// <summary>
/// Service for displaying folder picker dialogs.
/// </summary>
public interface IFolderDialogService
{
    /// <summary>
    /// Opens a folder picker dialog.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <returns>The selected folder path, or null if cancelled.</returns>
    public Task<string?> PickFolderAsync(Window owner);
}
