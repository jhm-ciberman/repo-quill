using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace RepoQuill.Gui.Services;

/// <summary>
/// Avalonia implementation of folder dialog service.
/// </summary>
public class FolderDialogService : IFolderDialogService
{
    public async Task<string?> PickFolderAsync(Window owner)
    {
        var storageProvider = owner.StorageProvider;
        var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Repository Folder",
            AllowMultiple = false
        });

        if (result.Count > 0)
        {
            return result[0].Path.LocalPath;
        }

        return null;
    }
}
