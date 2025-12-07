namespace RepoQuill.Core.Utilities;

/// <summary>
/// Detects whether a file is binary (non-text).
/// </summary>
public static class BinaryDetector
{
    /// <summary>
    /// File extensions known to be binary.
    /// </summary>
    private static readonly HashSet<string> BinaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Executables and libraries
        ".exe", ".dll", ".so", ".dylib", ".bin", ".obj", ".o", ".a", ".lib",

        // Images
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".webp", ".tiff", ".tif",
        ".psd", ".raw", ".heic", ".heif",

        // Audio
        ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a",

        // Video
        ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv", ".webm", ".m4v",

        // Archives
        ".zip", ".tar", ".gz", ".7z", ".rar", ".bz2", ".xz", ".zst",

        // Documents (binary formats)
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",

        // Fonts
        ".woff", ".woff2", ".ttf", ".otf", ".eot",

        // Compiled/bytecode
        ".pyc", ".pyo", ".class", ".pdb", ".nupkg", ".snupkg",

        // Database files
        ".db", ".sqlite", ".sqlite3", ".mdb",

        // Other binary formats
        ".iso", ".dmg", ".pkg", ".deb", ".rpm",
        ".jar", ".war", ".ear",
        ".node", ".wasm"
    };

    /// <summary>
    /// Number of bytes to read when checking for null bytes.
    /// </summary>
    private const int BytesToCheck = 8192;

    /// <summary>
    /// Checks if a file is binary based on its extension.
    /// </summary>
    public static bool IsBinaryExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return !string.IsNullOrEmpty(extension) && BinaryExtensions.Contains(extension);
    }

    /// <summary>
    /// Checks if a file is binary by looking for null bytes in its content.
    /// This is more accurate but requires reading the file.
    /// </summary>
    public static bool ContainsNullBytes(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var buffer = new byte[Math.Min(BytesToCheck, stream.Length)];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);

            for (int i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0)
                    return true;
            }

            return false;
        }
        catch
        {
            // If we can't read the file, assume it's not binary
            // The actual error will be caught during content loading
            return false;
        }
    }

    /// <summary>
    /// Checks if a file is binary using both extension and content analysis.
    /// Extension check is performed first for efficiency.
    /// </summary>
    public static bool IsBinary(string filePath)
    {
        return IsBinaryExtension(filePath) || ContainsNullBytes(filePath);
    }

    /// <summary>
    /// Checks if a file is binary based on extension only (fast, no I/O).
    /// </summary>
    public static bool IsBinaryFast(string filePath)
    {
        return IsBinaryExtension(filePath);
    }
}
