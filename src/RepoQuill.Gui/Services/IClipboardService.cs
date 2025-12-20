using System.Threading.Tasks;

namespace RepoQuill.Gui.Services;

/// <summary>
/// Service for clipboard operations.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Sets text to the clipboard.
    /// </summary>
    public Task SetTextAsync(string text);
}
