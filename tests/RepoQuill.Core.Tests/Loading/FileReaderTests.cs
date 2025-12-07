using RepoQuill.Core.Loading;
using RepoQuill.Core.Models;
using RepoQuill.Core.Tests.TestFixtures;

namespace RepoQuill.Core.Tests.Loading;

public sealed class FileReaderTests : IDisposable
{
    private readonly TestFileSystemFixture _fixture;
    private readonly FileReader _reader;

    public FileReaderTests()
    {
        this._fixture = new TestFileSystemFixture();
        this._reader = new FileReader();
    }

    [Fact]
    public async Task LoadAsync_ReadsUtf8File()
    {
        // Arrange
        var content = "Hello, World! 你好世界";
        this._fixture.CreateFile("test.txt", content);
        var entry = this.CreateEntry("test.txt");

        // Act
        var result = await this._reader.LoadAsync(entry);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(content, result.Value!.Content);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ReturnsError()
    {
        // Arrange
        var entry = this.CreateEntry("nonexistent.txt");

        // Act
        var result = await this._reader.LoadAsync(entry);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains("not found", result.Error.Message.ToLower());
    }

    [Fact]
    public async Task LoadAsync_PreservesFileEntry()
    {
        // Arrange
        this._fixture.CreateFile("test.txt", "content");
        var entry = this.CreateEntry("test.txt");

        // Act
        var result = await this._reader.LoadAsync(entry);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(entry, result.Value!.Entry);
    }

    [Fact]
    public async Task LoadAsync_HandlesEmptyFile()
    {
        // Arrange
        this._fixture.CreateFile("empty.txt", "");
        var entry = this.CreateEntry("empty.txt");

        // Act
        var result = await this._reader.LoadAsync(entry);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("", result.Value!.Content);
    }

    private FileEntry CreateEntry(string relativePath) => new()
    {
        AbsolutePath = Path.Combine(this._fixture.RootPath, relativePath),
        RelativePath = relativePath,
        SizeBytes = 0,
        LastModified = DateTime.UtcNow
    };

    public void Dispose()
    {
        this._fixture.Dispose();
    }
}
