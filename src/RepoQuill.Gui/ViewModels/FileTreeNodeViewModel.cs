using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RepoQuill.Gui.Models;

namespace RepoQuill.Gui.ViewModels;

/// <summary>
/// ViewModel for a file or folder node in the tree.
/// </summary>
public partial class FileTreeNodeViewModel : ViewModelBase
{
    private FileNodeState _state = FileNodeState.Checked;
    private bool _isUpdatingState;

    public FileTreeNodeViewModel(
        string name,
        string relativePath,
        string absolutePath,
        bool isDirectory,
        bool isBinary,
        long sizeBytes,
        FileTreeNodeViewModel? parent)
    {
        this.Name = name;
        this.RelativePath = relativePath;
        this.AbsolutePath = absolutePath;
        this.IsDirectory = isDirectory;
        this.IsBinary = isBinary;
        this.SizeBytes = sizeBytes;
        this.Parent = parent;
        this.Children = [];

        // Binary files default to TreeOnly
        if (isBinary && !isDirectory)
        {
            this._state = FileNodeState.TreeOnly;
        }
    }

    public string Name { get; }

    public string RelativePath { get; }

    public string AbsolutePath { get; }

    public bool IsDirectory { get; }

    public bool IsBinary { get; }

    public long SizeBytes { get; }

    public FileTreeNodeViewModel? Parent { get; }

    public ObservableCollection<FileTreeNodeViewModel> Children { get; }

    public FileNodeState State
    {
        get => this._state;
        set
        {
            if (this.SetProperty(ref this._state, value))
            {
                this.OnPropertyChanged(nameof(this.IsTreeOnly));
            }
        }
    }

    public bool IsTreeOnly => this.State == FileNodeState.TreeOnly;

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isExpanded = true;

    /// <summary>
    /// Toggles the checkbox state (standard checkbox behavior).
    /// </summary>
    [RelayCommand]
    private void ToggleCheck()
    {
        if (this._isUpdatingState)
        {
            return;
        }

        var newState = this.State switch
        {
            FileNodeState.Checked => FileNodeState.Unchecked,
            FileNodeState.Unchecked => FileNodeState.Checked,
            FileNodeState.TreeOnly => FileNodeState.Checked,
            FileNodeState.Indeterminate => FileNodeState.Checked,
            _ => FileNodeState.Checked
        };

        this.SetStateRecursive(newState);
        this.RecalculateParentState();
    }

    /// <summary>
    /// Cycles through states (for advanced [T] icon toggle).
    /// Checked -> TreeOnly -> Unchecked -> Checked
    /// </summary>
    [RelayCommand]
    private void CycleState()
    {
        if (this._isUpdatingState)
        {
            return;
        }

        var newState = this.State switch
        {
            FileNodeState.Checked => FileNodeState.TreeOnly,
            FileNodeState.TreeOnly => FileNodeState.Unchecked,
            FileNodeState.Unchecked => FileNodeState.Checked,
            FileNodeState.Indeterminate => FileNodeState.Checked,
            _ => FileNodeState.Checked
        };

        this.SetStateRecursive(newState);
        this.RecalculateParentState();
    }

    /// <summary>
    /// Sets the state recursively for this node and all children.
    /// </summary>
    public void SetStateRecursive(FileNodeState state)
    {
        this._isUpdatingState = true;
        try
        {
            this.State = state;

            foreach (var child in this.Children)
            {
                child.SetStateRecursive(state);
            }
        }
        finally
        {
            this._isUpdatingState = false;
        }
    }

    /// <summary>
    /// Recalculates and updates parent states based on children.
    /// </summary>
    public void RecalculateParentState()
    {
        this.Parent?.UpdateStateFromChildren();
    }

    private void UpdateStateFromChildren()
    {
        if (this.Children.Count == 0)
        {
            return;
        }

        this._isUpdatingState = true;
        try
        {
            var childStates = this.Children
                .Select(c => c.State)
                .Distinct()
                .ToList();

            if (childStates.Count == 1)
            {
                // All children have the same state
                this.State = childStates[0];
            }
            else
            {
                // Mixed children
                this.State = FileNodeState.Indeterminate;
            }
        }
        finally
        {
            this._isUpdatingState = false;
        }

        // Continue up the tree
        this.Parent?.UpdateStateFromChildren();
    }

    /// <summary>
    /// Updates visibility based on filter options.
    /// </summary>
    public void UpdateVisibility(bool hideBinaries)
    {
        if (this.IsDirectory)
        {
            // Directory visibility depends on whether it has any visible children
            foreach (var child in this.Children)
            {
                child.UpdateVisibility(hideBinaries);
            }

            this.IsVisible = this.Children.Any(c => c.IsVisible);
        }
        else
        {
            this.IsVisible = !(hideBinaries && this.IsBinary);
        }
    }

    /// <summary>
    /// Gets all descendant file nodes (not directories).
    /// </summary>
    public IEnumerable<FileTreeNodeViewModel> GetAllFileNodes()
    {
        if (!this.IsDirectory)
        {
            yield return this;
        }

        foreach (var child in this.Children)
        {
            foreach (var node in child.GetAllFileNodes())
            {
                yield return node;
            }
        }
    }

    /// <summary>
    /// Gets all nodes (files and directories).
    /// </summary>
    public IEnumerable<FileTreeNodeViewModel> GetAllNodes()
    {
        yield return this;

        foreach (var child in this.Children)
        {
            foreach (var node in child.GetAllNodes())
            {
                yield return node;
            }
        }
    }
}
