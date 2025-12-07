using System.Text;
using RepoQuill.Core.Models;

namespace RepoQuill.Core.Loading;

/// <summary>
/// Reads file content with encoding detection.
/// </summary>
public sealed class FileReader : IContentLoader
{
    /// <inheritdoc/>
    public async Task<Result<FileContent>> LoadAsync(FileEntry entry, CancellationToken ct = default)
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var content = await ReadWithEncodingDetectionAsync(entry.AbsolutePath, ct);

            return Result<FileContent>.Success(new FileContent(entry, content));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (FileNotFoundException)
        {
            return Result<FileContent>.Failure(entry.RelativePath, "File not found");
        }
        catch (UnauthorizedAccessException)
        {
            return Result<FileContent>.Failure(entry.RelativePath, "Access denied");
        }
        catch (IOException ex)
        {
            return Result<FileContent>.Failure(entry.RelativePath, $"IO error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<FileContent>.Failure(entry.RelativePath, $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads a file with encoding detection from BOM, defaulting to UTF-8.
    /// </summary>
    private async Task<string> ReadWithEncodingDetectionAsync(string filePath, CancellationToken ct)
    {
        // First, try to detect encoding from BOM
        var encoding = await DetectEncodingFromBomAsync(filePath, ct);

        // Read with detected encoding (or UTF-8 default)
        try
        {
            return await File.ReadAllTextAsync(filePath, encoding, ct);
        }
        catch (DecoderFallbackException)
        {
            // If UTF-8 fails, try with system default encoding
            return await File.ReadAllTextAsync(filePath, Encoding.Default, ct);
        }
    }

    /// <summary>
    /// Detects encoding from BOM (Byte Order Mark).
    /// Returns UTF-8 if no BOM is found.
    /// </summary>
    private async Task<Encoding> DetectEncodingFromBomAsync(string filePath, CancellationToken ct)
    {
        try
        {
            var buffer = new byte[4];
            await using var stream = File.OpenRead(filePath);
            var bytesRead = await stream.ReadAsync(buffer, ct);

            if (bytesRead >= 2)
            {
                // UTF-16 BE BOM
                if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                    return Encoding.BigEndianUnicode;

                // UTF-16 LE BOM
                if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    // Check for UTF-32 LE
                    if (bytesRead >= 4 && buffer[2] == 0x00 && buffer[3] == 0x00)
                        return Encoding.UTF32;
                    return Encoding.Unicode;
                }

                // UTF-8 BOM
                if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                    return Encoding.UTF8;

                // UTF-32 BE BOM
                if (bytesRead >= 4 && buffer[0] == 0x00 && buffer[1] == 0x00 &&
                    buffer[2] == 0xFE && buffer[3] == 0xFF)
                {
                    return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
                }
            }
        }
        catch
        {
            // If we can't read the BOM, fall back to UTF-8
        }

        // Default to UTF-8 without BOM
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}
