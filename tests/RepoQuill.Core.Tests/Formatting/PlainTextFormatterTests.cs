using RepoQuill.Core.Formatting;
using RepoQuill.Core.Models;

namespace RepoQuill.Core.Tests.Formatting;

public sealed class PlainTextFormatterTests
{
    private readonly PlainTextFormatter _formatter = new();

    private static FileEntry CreateEntry(string relativePath, long sizeBytes = 100) => new()
    {
        AbsolutePath = $"C:/test/{relativePath}",
        RelativePath = relativePath,
        SizeBytes = sizeBytes,
        LastModified = DateTime.UtcNow
    };

    private static FileContent CreateContent(string relativePath, string content) => new(
        CreateEntry(relativePath, content.Length),
        content);

    [Fact]
    public void Format_IncludesTreeSection()
    {
        // Arrange
        var files = new List<FileEntry> { CreateEntry("test.cs") };
        var contents = new List<FileContent> { CreateContent("test.cs", "code") };

        // Act
        var result = this._formatter.Format(files, contents);

        // Assert
        Assert.Contains("PROJECT STRUCTURE", result);
        Assert.Contains("test.cs", result);
    }

    [Fact]
    public void Format_IncludesFileContent()
    {
        // Arrange
        var files = new List<FileEntry> { CreateEntry("test.cs") };
        var contents = new List<FileContent> { CreateContent("test.cs", "var x = 1;") };

        // Act
        var result = this._formatter.Format(files, contents);

        // Assert
        Assert.Contains("FILES", result);
        Assert.Contains("var x = 1;", result);
    }

    [Fact]
    public void Format_ShowsFilePath()
    {
        // Arrange
        var files = new List<FileEntry> { CreateEntry("src/app.cs") };
        var contents = new List<FileContent> { CreateContent("src/app.cs", "code") };

        // Act
        var result = this._formatter.Format(files, contents);

        // Assert
        Assert.Contains("File: src/app.cs", result);
    }

    [Fact]
    public void Format_ShowsFileSize()
    {
        // Arrange
        var files = new List<FileEntry> { CreateEntry("test.cs", 1500) };
        var contents = new List<FileContent>
        {
            new(files[0], new string('x', 1500))
        };

        // Act
        var result = this._formatter.Format(files, contents);

        // Assert
        Assert.Contains("KB", result);
    }

    [Fact]
    public void Format_NoContent_OnlyShowsTree()
    {
        // Arrange
        var files = new List<FileEntry> { CreateEntry("image.png") };
        var contents = new List<FileContent>();

        // Act
        var result = this._formatter.Format(files, contents);

        // Assert
        Assert.Contains("PROJECT STRUCTURE", result);
        Assert.DoesNotContain("FILES", result);
    }

    [Fact]
    public void Format_MultipleFiles_SeparatesWithDividers()
    {
        // Arrange
        var files = new List<FileEntry>
        {
            CreateEntry("file1.cs"),
            CreateEntry("file2.cs")
        };
        var contents = new List<FileContent>
        {
            CreateContent("file1.cs", "content1"),
            CreateContent("file2.cs", "content2")
        };

        // Act
        var result = this._formatter.Format(files, contents);

        // Assert
        Assert.Contains("file1.cs", result);
        Assert.Contains("file2.cs", result);
        Assert.Contains("content1", result);
        Assert.Contains("content2", result);
    }
}
