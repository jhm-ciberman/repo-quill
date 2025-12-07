using System.Text;
using RepoQuill.Core.Models;
using RepoQuill.Core.Tree;

namespace RepoQuill.Core.Formatting;

/// <summary>
/// Formats output as plain text with tree and content sections.
/// </summary>
public sealed class PlainTextFormatter : IOutputFormatter
{
    private const string SectionSeparator = "================================================================================";
    private const string FileSeparator = "────────────────────────────────────────────────────────────────────────────────";

    private readonly ITreeRenderer _treeRenderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainTextFormatter"/> class with the default ASCII tree renderer.
    /// </summary>
    public PlainTextFormatter() : this(new AsciiTreeRenderer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlainTextFormatter"/> class with a custom tree renderer.
    /// </summary>
    /// <param name="treeRenderer">The tree renderer to use for generating the directory tree.</param>
    public PlainTextFormatter(ITreeRenderer treeRenderer)
    {
        _treeRenderer = treeRenderer;
    }

    /// <inheritdoc/>
    public string Format(IReadOnlyList<FileEntry> allFiles, IReadOnlyList<FileContent> contents)
    {
        var sb = new StringBuilder();

        // Tree section
        AppendTreeSection(sb, allFiles);

        // Content section
        if (contents.Count > 0)
        {
            sb.AppendLine();
            AppendContentSection(sb, contents);
        }

        return sb.ToString();
    }

    private void AppendTreeSection(StringBuilder sb, IReadOnlyList<FileEntry> files)
    {
        sb.AppendLine(SectionSeparator);
        sb.AppendLine("                              PROJECT STRUCTURE");
        sb.AppendLine(SectionSeparator);
        sb.AppendLine();

        var tree = _treeRenderer.Render(files, string.Empty);
        sb.AppendLine(tree);
    }

    private void AppendContentSection(StringBuilder sb, IReadOnlyList<FileContent> contents)
    {
        sb.AppendLine(SectionSeparator);
        sb.AppendLine("                                   FILES");
        sb.AppendLine(SectionSeparator);

        foreach (var content in contents)
        {
            sb.AppendLine();
            sb.AppendLine(FileSeparator);
            sb.Append("File: ");
            sb.Append(content.Entry.RelativePath);
            sb.Append(" (");
            sb.Append(FormatFileSize(content.Entry.SizeBytes));
            sb.AppendLine(")");
            sb.AppendLine(FileSeparator);
            sb.AppendLine();
            sb.AppendLine(content.Content);
        }
    }

    private static string FormatFileSize(long bytes)
    {
        const long kb = 1024;
        const long mb = kb * 1024;

        return bytes switch
        {
            < kb => $"{bytes} B",
            < mb => $"{bytes / (double)kb:F1} KB",
            _ => $"{bytes / (double)mb:F1} MB"
        };
    }
}
