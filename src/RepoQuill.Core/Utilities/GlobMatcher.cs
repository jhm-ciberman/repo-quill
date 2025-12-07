using System.Text.RegularExpressions;

namespace RepoQuill.Core.Utilities;

/// <summary>
/// Matches file paths against glob patterns.
/// Supports *, **, and ? wildcards.
/// </summary>
public static class GlobMatcher
{
    /// <summary>
    /// Checks if a path matches any of the given glob patterns.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <param name="patterns">The glob patterns to match against.</param>
    /// <returns><c>true</c> if the path matches any pattern; otherwise, <c>false</c>.</returns>
    public static bool MatchesAny(string path, IEnumerable<string> patterns)
    {
        return patterns.Any(pattern => Matches(path, pattern));
    }

    /// <summary>
    /// Checks if a path matches a glob pattern.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <param name="pattern">The glob pattern to match against.</param>
    /// <returns><c>true</c> if the path matches the pattern; otherwise, <c>false</c>.</returns>
    public static bool Matches(string path, string pattern)
    {
        // Normalize path separators
        path = NormalizePath(path);
        pattern = NormalizePath(pattern);

        // If pattern has no directory separator, match against filename only
        // OR match as a suffix anywhere in the path
        if (!pattern.Contains('/'))
        {
            var fileName = Path.GetFileName(path);
            var regex = GlobToRegex(pattern, anchorStart: true, anchorEnd: true);
            return regex.IsMatch(fileName);
        }

        // Pattern contains path separators - match full path
        var fullRegex = GlobToRegex(pattern, anchorStart: true, anchorEnd: true);
        return fullRegex.IsMatch(path);
    }

    /// <summary>
    /// Converts a glob pattern to a regex pattern.
    /// </summary>
    private static Regex GlobToRegex(string pattern, bool anchorStart, bool anchorEnd)
    {
        var regexPattern = anchorStart ? "^" : "";

        // Handle leading **/ which means match in any directory
        if (pattern.StartsWith("**/"))
        {
            regexPattern = "^(.*/)?";
            pattern = pattern[3..];
        }

        var i = 0;
        while (i < pattern.Length)
        {
            var c = pattern[i];

            switch (c)
            {
                case '*':
                    if (i + 1 < pattern.Length && pattern[i + 1] == '*')
                    {
                        // ** matches any path (including directory separators)
                        if (i + 2 < pattern.Length && pattern[i + 2] == '/')
                        {
                            // **/ matches zero or more directories
                            regexPattern += "(.*/)?";
                            i += 3;
                        }
                        else
                        {
                            // ** at end or not followed by / matches everything
                            regexPattern += ".*";
                            i += 2;
                        }
                    }
                    else
                    {
                        // * matches anything except /
                        regexPattern += "[^/]*";
                        i++;
                    }
                    break;

                case '?':
                    // ? matches any single character except /
                    regexPattern += "[^/]";
                    i++;
                    break;

                case '.':
                case '(':
                case ')':
                case '{':
                case '}':
                case '[':
                case ']':
                case '+':
                case '^':
                case '$':
                case '|':
                case '\\':
                    // Escape regex special characters
                    regexPattern += "\\" + c;
                    i++;
                    break;

                default:
                    regexPattern += c;
                    i++;
                    break;
            }
        }

        if (anchorEnd)
        {
            regexPattern += "$";
        }

        return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    /// <summary>
    /// Normalizes a path to use forward slashes and removes leading ./
    /// </summary>
    private static string NormalizePath(string path)
    {
        path = path.Replace('\\', '/');

        // Remove leading ./
        if (path.StartsWith("./"))
        {
            path = path[2..];
        }

        // Remove trailing /
        path = path.TrimEnd('/');

        return path;
    }
}
