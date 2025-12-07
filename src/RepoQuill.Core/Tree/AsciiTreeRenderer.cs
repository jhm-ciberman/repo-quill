using System.Text;
using RepoQuill.Core.Models;

namespace RepoQuill.Core.Tree;

/// <summary>
/// Renders a traditional ASCII tree view.
/// </summary>
public sealed class AsciiTreeRenderer : ITreeRenderer
{
    private const string Branch = "├── ";
    private const string LastBranch = "└── ";
    private const string Vertical = "│   ";
    private const string Empty = "    ";

    /// <inheritdoc/>
    public string Render(IReadOnlyList<FileEntry> files, string rootPath)
    {
        if (files.Count == 0)
            return string.Empty;

        // Build a tree structure from the flat list of files
        var root = BuildTree(files);

        // Render the tree
        var sb = new StringBuilder();
        RenderNode(root, sb, "", true);

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Builds a tree structure from a flat list of file entries.
    /// </summary>
    private TreeNode BuildTree(IReadOnlyList<FileEntry> files)
    {
        var root = new TreeNode("", isDirectory: true);

        foreach (var file in files)
        {
            var parts = file.RelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var isLast = i == parts.Length - 1;

                if (isLast)
                {
                    // This is the file
                    current.Children.Add(new TreeNode(part, isDirectory: false, fileEntry: file));
                }
                else
                {
                    // This is a directory
                    var existingDir = current.Children.FirstOrDefault(c => c.Name == part && c.IsDirectory);
                    if (existingDir == null)
                    {
                        existingDir = new TreeNode(part, isDirectory: true);
                        current.Children.Add(existingDir);
                    }
                    current = existingDir;
                }
            }
        }

        // Sort children: directories first, then files, both alphabetically
        SortChildren(root);

        return root;
    }

    /// <summary>
    /// Recursively sorts children of a node.
    /// </summary>
    private void SortChildren(TreeNode node)
    {
        node.Children.Sort((a, b) =>
        {
            // Directories first
            if (a.IsDirectory != b.IsDirectory)
                return a.IsDirectory ? -1 : 1;
            // Then alphabetically
            return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        });

        foreach (var child in node.Children)
        {
            if (child.IsDirectory)
                SortChildren(child);
        }
    }

    /// <summary>
    /// Renders a node and its children.
    /// </summary>
    private void RenderNode(TreeNode node, StringBuilder sb, string prefix, bool isRoot)
    {
        if (!isRoot)
        {
            // Don't render the root node itself
            return;
        }

        // Render children of root
        for (int i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var isLast = i == node.Children.Count - 1;
            RenderChild(child, sb, "", isLast);
        }
    }

    /// <summary>
    /// Renders a child node.
    /// </summary>
    private void RenderChild(TreeNode node, StringBuilder sb, string prefix, bool isLast)
    {
        var branch = isLast ? LastBranch : Branch;

        sb.Append(prefix);
        sb.Append(branch);

        if (node.IsDirectory)
        {
            sb.Append(node.Name);
            sb.Append('/');
            sb.AppendLine();

            var newPrefix = prefix + (isLast ? Empty : Vertical);
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                var childIsLast = i == node.Children.Count - 1;
                RenderChild(child, sb, newPrefix, childIsLast);
            }
        }
        else
        {
            sb.Append(node.Name);

            // Add marker for tree-only files
            if (node.FileEntry?.State == FileState.TreeOnly)
            {
                sb.Append("  [tree-only]");
            }

            sb.AppendLine();
        }
    }

    /// <summary>
    /// Represents a node in the file tree.
    /// </summary>
    private class TreeNode
    {
        public string Name { get; }
        public bool IsDirectory { get; }
        public FileEntry? FileEntry { get; }
        public List<TreeNode> Children { get; } = new();

        public TreeNode(string name, bool isDirectory, FileEntry? fileEntry = null)
        {
            Name = name;
            IsDirectory = isDirectory;
            FileEntry = fileEntry;
        }
    }
}
