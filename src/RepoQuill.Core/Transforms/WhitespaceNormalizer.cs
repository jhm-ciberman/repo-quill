using System.Text.RegularExpressions;
using RepoQuill.Core.Models;

namespace RepoQuill.Core.Transforms;

/// <summary>
/// Normalizes whitespace in file content.
/// </summary>
public sealed class WhitespaceNormalizer : IContentTransform
{
    /// <inheritdoc/>
    public FileContent Transform(FileContent content)
    {
        var normalized = NormalizeWhitespace(content.Content);
        return content with { Content = normalized };
    }

    private string NormalizeWhitespace(string content)
    {
        // Normalize line endings to LF
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        // Remove trailing whitespace from each line
        content = Regex.Replace(content, @"[ \t]+$", string.Empty, RegexOptions.Multiline);

        // Collapse multiple blank lines into a single blank line
        content = Regex.Replace(content, @"\n{3,}", "\n\n");

        // Remove trailing newlines at end of file, keep exactly one
        content = content.TrimEnd() + "\n";

        return content;
    }
}
