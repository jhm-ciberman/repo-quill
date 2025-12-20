using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RepoQuill.Gui.Models;

namespace RepoQuill.Gui.Services;

/// <summary>
/// Persists settings to a JSON file in the user's AppData folder.
/// </summary>
public class SettingsService : ISettingsService, IDisposable
{
    private const int MaxRecentFolders = 10;
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RepoQuill");
    private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");

    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<RecentFolder>> GetRecentFoldersAsync()
    {
        await this._lock.WaitAsync();
        try
        {
            var settings = await this.LoadSettingsAsync();
            return settings.RecentFolders;
        }
        finally
        {
            this._lock.Release();
        }
    }

    public async Task AddRecentFolderAsync(string path)
    {
        await this._lock.WaitAsync();
        try
        {
            var settings = await this.LoadSettingsAsync();
            var folders = settings.RecentFolders.ToList();

            // Remove if already exists
            folders.RemoveAll(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase));

            // Add to top
            folders.Insert(0, new RecentFolder(path, DateTime.UtcNow));

            // Trim to max
            if (folders.Count > MaxRecentFolders)
            {
                folders = folders.Take(MaxRecentFolders).ToList();
            }

            settings = settings with { RecentFolders = folders };
            await this.SaveSettingsAsync(settings);
        }
        finally
        {
            this._lock.Release();
        }
    }

    private async Task<AppSettings> LoadSettingsAsync()
    {
        if (!File.Exists(SettingsFile))
        {
            return new AppSettings();
        }

        try
        {
            var json = await File.ReadAllTextAsync(SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private async Task SaveSettingsAsync(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsFolder);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(SettingsFile, json);
    }

    private record AppSettings
    {
        public List<RecentFolder> RecentFolders { get; init; } = [];
    }

    public void Dispose()
    {
        this._lock.Dispose();
        GC.SuppressFinalize(this);
    }
}
