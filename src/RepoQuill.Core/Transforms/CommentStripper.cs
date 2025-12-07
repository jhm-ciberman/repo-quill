using System.Text.RegularExpressions;
using RepoQuill.Core.Models;

namespace RepoQuill.Core.Transforms;

/// <summary>
/// Strips comments from source files based on file extension.
/// </summary>
public sealed class CommentStripper : IContentTransform
{
    /// <inheritdoc/>
    public FileContent Transform(FileContent content)
    {
        var extension = Path.GetExtension(content.Entry.RelativePath).ToLowerInvariant();
        var strippedContent = StripComments(content.Content, extension);
        return content with { Content = strippedContent };
    }

    private string StripComments(string content, string extension)
    {
        return extension switch
        {
            // C-style comments: // and /* */
            ".cs" or ".java" or ".js" or ".ts" or ".tsx" or ".jsx" or
            ".c" or ".cpp" or ".cc" or ".cxx" or ".h" or ".hpp" or
            ".go" or ".swift" or ".kt" or ".kts" or ".scala" or
            ".rs" or ".m" or ".mm"
                => StripCStyleComments(content),

            // Hash comments: #
            ".py" or ".rb" or ".pl" or ".pm" or ".sh" or ".bash" or
            ".zsh" or ".fish" or ".ps1" or ".psm1" or ".r" or
            ".yaml" or ".yml" or ".toml" or ".conf" or ".ini" or
            ".dockerfile" or ".makefile" or ".mk"
                => StripHashComments(content),

            // XML/HTML comments: <!-- -->
            ".html" or ".htm" or ".xml" or ".xaml" or ".axaml" or
            ".svg" or ".xsl" or ".xslt" or ".xsd" or ".wsdl" or
            ".csproj" or ".fsproj" or ".vbproj" or ".props" or ".targets"
                => StripXmlComments(content),

            // SQL comments: -- and /* */
            ".sql"
                => StripSqlComments(content),

            // Lua comments: -- and --[[ ]]
            ".lua"
                => StripLuaComments(content),

            // CSS comments: /* */
            ".css" or ".scss" or ".sass" or ".less"
                => StripCssComments(content),

            // No comment stripping for unknown extensions
            _ => content
        };
    }

    private string StripCStyleComments(string content)
    {
        // Remove multi-line comments /* */
        content = Regex.Replace(content, @"/\*[\s\S]*?\*/", string.Empty);

        // Remove single-line comments //
        // Be careful not to remove URLs (http://, https://)
        content = Regex.Replace(content, @"(?<!:)//.*$", string.Empty, RegexOptions.Multiline);

        return content;
    }

    private string StripHashComments(string content)
    {
        // Remove # comments (but not shebang on first line)
        var lines = content.Split('\n');
        var result = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Keep shebang on first line
            if (i == 0 && line.TrimStart().StartsWith("#!"))
            {
                result.Add(line);
                continue;
            }

            // Remove comment portion
            var commentIndex = line.IndexOf('#');
            if (commentIndex >= 0)
            {
                // Check if # is inside a string (simple heuristic)
                var beforeComment = line[..commentIndex];
                var singleQuotes = beforeComment.Count(c => c == '\'');
                var doubleQuotes = beforeComment.Count(c => c == '"');

                // If odd number of quotes, we're likely inside a string
                if (singleQuotes % 2 == 0 && doubleQuotes % 2 == 0)
                {
                    line = beforeComment;
                }
            }

            result.Add(line);
        }

        return string.Join('\n', result);
    }

    private string StripXmlComments(string content)
    {
        // Remove <!-- --> comments
        return Regex.Replace(content, @"<!--[\s\S]*?-->", string.Empty);
    }

    private string StripSqlComments(string content)
    {
        // Remove multi-line comments /* */
        content = Regex.Replace(content, @"/\*[\s\S]*?\*/", string.Empty);

        // Remove single-line comments --
        content = Regex.Replace(content, @"--.*$", string.Empty, RegexOptions.Multiline);

        return content;
    }

    private string StripLuaComments(string content)
    {
        // Remove multi-line comments --[[ ]]
        content = Regex.Replace(content, @"--\[\[[\s\S]*?\]\]", string.Empty);

        // Remove single-line comments --
        content = Regex.Replace(content, @"--.*$", string.Empty, RegexOptions.Multiline);

        return content;
    }

    private string StripCssComments(string content)
    {
        // Remove /* */ comments
        return Regex.Replace(content, @"/\*[\s\S]*?\*/", string.Empty);
    }
}
