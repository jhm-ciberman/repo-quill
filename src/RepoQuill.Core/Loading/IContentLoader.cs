using RepoQuill.Core.Models;

namespace RepoQuill.Core.Loading;

/// <summary>
/// Loads file content from disk.
/// </summary>
public interface IContentLoader
{
    /// <summary>
    /// Loads the content of a file.
    /// </summary>
    /// <param name="entry">The file entry to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the file content or an error.</returns>
    Task<Result<FileContent>> LoadAsync(FileEntry entry, CancellationToken ct = default);
}

/// <summary>
/// Represents the result of an operation that can fail.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public record Result<T>
{
    /// <summary>
    /// Gets the success value, if the operation succeeded.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Gets the error, if the operation failed.
    /// </summary>
    public FileError? Error { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Creates a success result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful result containing the value.</returns>
    public static Result<T> Success(T value) => new() { Value = value };

    /// <summary>
    /// Creates a failure result with the specified error information.
    /// </summary>
    /// <param name="filePath">The path of the file that caused the error.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result containing the error.</returns>
    public static Result<T> Failure(string filePath, string message) =>
        new() { Error = new FileError(filePath, message) };
}
