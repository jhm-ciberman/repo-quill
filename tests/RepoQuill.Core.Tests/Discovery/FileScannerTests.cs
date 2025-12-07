using RepoQuill.Core.Discovery;
using RepoQuill.Core.Models;
using RepoQuill.Core.Tests.TestFixtures;

namespace RepoQuill.Core.Tests.Discovery;

public sealed class FileScannerTests : IDisposable
{
    private readonly TestFileSystemFixture _fixture;
    private readonly FileScanner _scanner;

    public FileScannerTests()
    {
        this._fixture = new TestFileSystemFixture();
        this._scanner = new FileScanner();
    }

    [Fact]
    public async Task DiscoverAsync_FindsAllFiles()
    {
        // Arrange
        this._fixture.CreateFile("file1.txt", "content1");
        this._fixture.CreateFile("src/file2.cs", "content2");
        this._fixture.CreateFile("src/nested/file3.js", "content3");

        // Act
        var files = await ToListAsync(this._scanner.DiscoverAsync(this._fixture.RootPath, honorGitIgnore: false));

        // Assert
        Assert.Equal(3, files.Count);
        Assert.Contains(files, f => f.RelativePath == "file1.txt");
        Assert.Contains(files, f => f.RelativePath == "src/file2.cs");
        Assert.Contains(files, f => f.RelativePath == "src/nested/file3.js");
    }

    [Fact]
    public async Task DiscoverAsync_RespectsGitIgnore()
    {
        // Arrange
        this._fixture.CreateFile("file1.txt", "content1");
        this._fixture.CreateFile("ignored.log", "log content");
        this._fixture.CreateFile("build/output.dll", "binary");
        this._fixture.CreateGitIgnore("", "*.log", "build/");

        // Act
        var files = await ToListAsync(this._scanner.DiscoverAsync(this._fixture.RootPath, honorGitIgnore: true));

        // Assert
        Assert.Single(files);
        Assert.Equal("file1.txt", files[0].RelativePath);
    }

    [Fact]
    public async Task DiscoverAsync_RespectsNestedGitIgnore()
    {
        // Arrange
        this._fixture.CreateFile("root.txt", "root");
        this._fixture.CreateFile("src/code.cs", "code");
        this._fixture.CreateFile("src/temp.tmp", "temp");
        this._fixture.CreateGitIgnore("", "*.log");
        this._fixture.CreateGitIgnore("src", "*.tmp");

        // Act
        var files = await ToListAsync(this._scanner.DiscoverAsync(this._fixture.RootPath, honorGitIgnore: true));

        // Assert
        Assert.Equal(2, files.Count);
        Assert.Contains(files, f => f.RelativePath == "root.txt");
        Assert.Contains(files, f => f.RelativePath == "src/code.cs");
    }

    [Fact]
    public async Task DiscoverAsync_SkipsHiddenDirectories()
    {
        // Arrange
        this._fixture.CreateFile("visible.txt", "visible");
        this._fixture.CreateFile(".hidden/secret.txt", "secret");

        // Act
        var files = await ToListAsync(this._scanner.DiscoverAsync(this._fixture.RootPath, honorGitIgnore: false));

        // Assert
        Assert.Single(files);
        Assert.Equal("visible.txt", files[0].RelativePath);
    }

    [Fact]
    public async Task DiscoverAsync_ReturnsCorrectMetadata()
    {
        // Arrange
        var content = "Hello, World!";
        this._fixture.CreateFile("test.txt", content);

        // Act
        var files = await ToListAsync(this._scanner.DiscoverAsync(this._fixture.RootPath, honorGitIgnore: false));

        // Assert
        var file = Assert.Single(files);
        Assert.Equal("test.txt", file.RelativePath);
        Assert.Equal(content.Length, file.SizeBytes);
        Assert.True(file.LastModified <= DateTime.UtcNow);
    }

    public void Dispose()
    {
        this._fixture.Dispose();
    }

    private static async Task<List<FileEntry>> ToListAsync(IAsyncEnumerable<FileEntry> source)
    {
        var list = new List<FileEntry>();
        await foreach (var item in source)
        {
            list.Add(item);
        }

        return list;
    }
}
