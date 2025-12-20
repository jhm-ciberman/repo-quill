using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RepoQuill.Core;
using RepoQuill.Core.Discovery;
using RepoQuill.Core.Models;
using RepoQuill.Core.Utilities;
using RepoQuill.Gui.Models;
using RepoQuill.Gui.Services;

namespace RepoQuill.Gui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly ISettingsService _settingsService;
    private readonly IFolderDialogService _folderDialogService;
    private readonly IClipboardService _clipboardService;
    private readonly FileScanner _fileScanner;
    private CancellationTokenSource? _cts;

    public MainWindowViewModel()
        : this(new SettingsService(), new FolderDialogService(), new ClipboardService())
    {
    }

    public MainWindowViewModel(
        ISettingsService settingsService,
        IFolderDialogService folderDialogService,
        IClipboardService clipboardService)
    {
        this._settingsService = settingsService;
        this._folderDialogService = folderDialogService;
        this._clipboardService = clipboardService;
        this._fileScanner = new FileScanner();
        this.RootNodes = [];
        this.RecentFolders = [];
        this.Options = new OptionsViewModel();

        this.OpenFolderCommand = new AsyncRelayCommand<Window>(this.OpenFolderAsync);
        this.OpenRecentFolderCommand = new AsyncRelayCommand<RecentFolder>(this.OpenRecentFolderAsync);
        this.SaveToFileCommand = new AsyncRelayCommand<Window>(this.SaveToFileAsync, _ => this.CanGenerate());
        this.CopyToClipboardCommand = new AsyncRelayCommand(this.CopyToClipboardAsync, this.CanGenerate);
        this.CancelCommand = new RelayCommand(this.Cancel);

        this.Options.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(OptionsViewModel.HideBinaries))
            {
                this.UpdateNodeVisibility();
            }
            else if (e.PropertyName == nameof(OptionsViewModel.HonorGitIgnore))
            {
                if (!string.IsNullOrEmpty(this.RootPath))
                {
                    await this.LoadFolderAsync(this.RootPath);
                }
            }
        };
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFolder))]
    [NotifyCanExecuteChangedFor(nameof(SaveToFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyToClipboardCommand))]
    private string? _rootPath;

    public bool HasFolder => !string.IsNullOrEmpty(this.RootPath);

    public ObservableCollection<RecentFolder> RecentFolders { get; }

    public ObservableCollection<FileTreeNodeViewModel> RootNodes { get; }

    public OptionsViewModel Options { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyToClipboardCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private int _progressPercent;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _statusMessage;

    public ICommand OpenFolderCommand { get; }
    public ICommand OpenRecentFolderCommand { get; }
    public IAsyncRelayCommand SaveToFileCommand { get; }
    public IAsyncRelayCommand CopyToClipboardCommand { get; }
    public ICommand CancelCommand { get; }

    public async Task InitializeAsync()
    {
        var folders = await this._settingsService.GetRecentFoldersAsync();
        this.RecentFolders.Clear();
        foreach (var folder in folders)
        {
            this.RecentFolders.Add(folder);
        }
    }

    private async Task OpenFolderAsync(Window? window)
    {
        if (window == null)
        {
            return;
        }

        var path = await this._folderDialogService.PickFolderAsync(window);
        if (!string.IsNullOrEmpty(path))
        {
            await this.LoadFolderAsync(path);
        }
    }

    private async Task OpenRecentFolderAsync(RecentFolder? folder)
    {
        if (folder == null)
        {
            return;
        }

        if (Directory.Exists(folder.Path))
        {
            await this.LoadFolderAsync(folder.Path);
        }
        else
        {
            this.ErrorMessage = $"Folder not found: {folder.Path}";
        }
    }

    private async Task LoadFolderAsync(string path)
    {
        this._cts?.Cancel();
        this._cts = new CancellationTokenSource();
        var ct = this._cts.Token;

        this.IsBusy = true;
        this.ProgressText = "Scanning folder...";
        this.ProgressPercent = 0;
        this.ErrorMessage = null;
        this.StatusMessage = null;

        try
        {
            this.RootPath = path;
            this.RootNodes.Clear();

            await this._settingsService.AddRecentFolderAsync(path);
            await this.InitializeAsync();

            var honorGitIgnore = this.Options.HonorGitIgnore;

            // Run file discovery on background thread
            var (rootNodes, fileCount) = await Task.Run(async () =>
            {
                var fileEntries = new List<FileEntry>();
                await foreach (var entry in this._fileScanner.DiscoverAsync(path, honorGitIgnore, ct))
                {
                    fileEntries.Add(entry);
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        this.ProgressText = $"Scanning: {entry.RelativePath}";
                    });
                }

                // Build tree structure on background thread
                var nodeMap = new Dictionary<string, FileTreeNodeViewModel>(StringComparer.OrdinalIgnoreCase);
                var roots = new List<FileTreeNodeViewModel>();

                foreach (var entry in fileEntries.OrderBy(e => e.RelativePath))
                {
                    ct.ThrowIfCancellationRequested();

                    var parts = entry.RelativePath.Split('/');
                    FileTreeNodeViewModel? parent = null;
                    var currentPath = "";

                    // Build directory nodes
                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        var part = parts[i];
                        currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                        if (!nodeMap.TryGetValue(currentPath, out var dirNode))
                        {
                            dirNode = new FileTreeNodeViewModel(
                                name: part,
                                relativePath: currentPath,
                                absolutePath: Path.Combine(path, currentPath.Replace('/', Path.DirectorySeparatorChar)),
                                isDirectory: true,
                                isBinary: false,
                                sizeBytes: 0,
                                parent: parent);

                            nodeMap[currentPath] = dirNode;

                            if (parent == null)
                            {
                                roots.Add(dirNode);
                            }
                            else
                            {
                                parent.Children.Add(dirNode);
                            }
                        }

                        parent = dirNode;
                    }

                    // Add file node
                    var fileName = parts[^1];
                    var isBinary = BinaryDetector.IsBinaryFast(entry.AbsolutePath);
                    var fileNode = new FileTreeNodeViewModel(
                        name: fileName,
                        relativePath: entry.RelativePath,
                        absolutePath: entry.AbsolutePath,
                        isDirectory: false,
                        isBinary: isBinary,
                        sizeBytes: entry.SizeBytes,
                        parent: parent);

                    nodeMap[entry.RelativePath] = fileNode;

                    if (parent == null)
                    {
                        roots.Add(fileNode);
                    }
                    else
                    {
                        parent.Children.Add(fileNode);
                    }
                }

                return (roots, fileEntries.Count);
            }, ct);

            // Update UI on UI thread
            foreach (var node in rootNodes)
            {
                this.RootNodes.Add(node);
            }

            // Update visibility based on options
            this.UpdateNodeVisibility();

            this.StatusMessage = $"Loaded {fileCount} files";
        }
        catch (OperationCanceledException)
        {
            this.StatusMessage = "Scan cancelled";
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error scanning folder: {ex.Message}";
        }
        finally
        {
            this.IsBusy = false;
            this.ProgressText = string.Empty;
            this.ProgressPercent = 0;
        }
    }

    private async Task SaveToFileAsync(Window? window)
    {
        if (window == null)
        {
            return;
        }

        var storageProvider = window.StorageProvider;
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Output",
            DefaultExtension = "txt",
            SuggestedFileName = "repo-output.txt",
            FileTypeChoices =
            [
                new FilePickerFileType("Text files") { Patterns = ["*.txt"] },
                new FilePickerFileType("All files") { Patterns = ["*.*"] }
            ]
        });

        if (file != null)
        {
            var output = await this.GenerateOutputAsync();
            if (output != null)
            {
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(output);
                this.StatusMessage = $"Saved to {file.Name}";
            }
        }
    }

    private async Task CopyToClipboardAsync()
    {
        var output = await this.GenerateOutputAsync();
        if (output != null)
        {
            await this._clipboardService.SetTextAsync(output);
            this.StatusMessage = "Copied to clipboard";
        }
    }

    private bool CanGenerate() => this.HasFolder && !this.IsBusy;

    private async Task<string?> GenerateOutputAsync()
    {
        this._cts?.Cancel();
        this._cts = new CancellationTokenSource();
        var ct = this._cts.Token;

        this.IsBusy = true;
        this.ErrorMessage = null;

        try
        {
            var config = this.BuildConfig();

            var progress = new Progress<ProgressReport>(report =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    this.ProgressPercent = report.ProgressPercent >= 0 ? report.ProgressPercent : 0;
                    this.ProgressText = $"{report.Phase}: {report.CurrentFile}";
                });
            });

            // Run on background thread to avoid blocking UI
            var result = await Task.Run(async () =>
            {
                var engine = new QuillEngine();
                return await engine.ExecuteAsync(config, progress, ct);
            }, ct);

            if (result.Errors.Count > 0)
            {
                this.ErrorMessage = $"{result.Errors.Count} files had errors";
            }

            this.StatusMessage = $"Generated: {result.FullFiles} files, {result.TreeOnlyFiles} tree-only";
            return result.Output;
        }
        catch (OperationCanceledException)
        {
            this.StatusMessage = "Generation cancelled";
            return null;
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error generating output: {ex.Message}";
            return null;
        }
        finally
        {
            this.IsBusy = false;
            this.ProgressText = string.Empty;
            this.ProgressPercent = 0;
        }
    }

    private QuillConfig BuildConfig()
    {
        var include = new List<string>();
        var exclude = new List<string>();
        var treeOnly = new List<string>();

        foreach (var node in this.GetAllFileNodes())
        {
            var path = node.RelativePath;
            switch (node.State)
            {
                case FileNodeState.Checked:
                    include.Add(path);
                    break;
                case FileNodeState.TreeOnly:
                    treeOnly.Add(path);
                    break;
                case FileNodeState.Unchecked:
                    exclude.Add(path);
                    break;
            }
        }

        return new QuillConfig
        {
            RootPath = this.RootPath!,
            IncludePatterns = include,
            ExcludePatterns = exclude,
            TreeOnlyPatterns = treeOnly,
            HonorGitIgnore = this.Options.HonorGitIgnore,
            StripComments = this.Options.StripComments,
            Format = OutputFormat.PlainText
        };
    }

    private IEnumerable<FileTreeNodeViewModel> GetAllFileNodes()
    {
        foreach (var root in this.RootNodes)
        {
            foreach (var node in root.GetAllFileNodes())
            {
                yield return node;
            }
        }
    }

    private void UpdateNodeVisibility()
    {
        foreach (var node in this.RootNodes)
        {
            node.UpdateVisibility(this.Options.HideBinaries);
        }
    }

    private void Cancel()
    {
        this._cts?.Cancel();
    }

    public void Dispose()
    {
        this._cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
