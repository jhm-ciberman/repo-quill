using RepoQuill.Core.Models;
using RepoQuill.Core.Tree;

namespace RepoQuill.Core.Tests.Tree;

public sealed class AsciiTreeRendererTests
{
    private readonly AsciiTreeRenderer _renderer = new();

    private static FileEntry CreateEntry(string relativePath, FileState state = FileState.Full) => new()
    {
        AbsolutePath = $"C:/test/{relativePath}",
        RelativePath = relativePath,
        SizeBytes = 100,
        LastModified = DateTime.UtcNow,
        State = state
    };

    [Fact]
    public void Render_SingleFile_RendersCorrectly()
    {
        // Arrange
        var files = new List<FileEntry> { CreateEntry("file.txt") };

        // Act
        var result = this._renderer.Render(files, "C:/test");

        // Assert
        Assert.Contains("file.txt", result);
    }

    [Fact]
    public void Render_NestedFiles_RendersTree()
    {
        // Arrange
        var files = new List<FileEntry>
        {
            CreateEntry("src/app.cs"),
            CreateEntry("src/utils/helper.cs"),
            CreateEntry("readme.md")
        };

        // Act
        var result = this._renderer.Render(files, "C:/test");

        // Assert
        Assert.Contains("src/", result);
        Assert.Contains("app.cs", result);
        Assert.Contains("utils/", result);
        Assert.Contains("helper.cs", result);
        Assert.Contains("readme.md", result);
    }

    [Fact]
    public void Render_TreeOnlyFiles_ShowsMarker()
    {
        // Arrange
        var files = new List<FileEntry>
        {
            CreateEntry("code.cs", FileState.Full),
            CreateEntry("image.png", FileState.TreeOnly)
        };

        // Act
        var result = this._renderer.Render(files, "C:/test");

        // Assert
        Assert.Contains("image.png", result);
        Assert.Contains("[tree-only]", result);
    }

    [Fact]
    public void Render_EmptyList_ReturnsEmpty()
    {
        // Arrange
        var files = new List<FileEntry>();

        // Act
        var result = this._renderer.Render(files, "C:/test");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Render_SortsDirectoriesFirst()
    {
        // Arrange
        var files = new List<FileEntry>
        {
            CreateEntry("zebra.txt"),
            CreateEntry("alpha/file.txt"),
            CreateEntry("beta.txt")
        };

        // Act
        var result = this._renderer.Render(files, "C:/test");

        // Assert
        var alphaIndex = result.IndexOf("alpha/");
        var zebraIndex = result.IndexOf("zebra.txt");
        var betaIndex = result.IndexOf("beta.txt");

        Assert.True(alphaIndex < zebraIndex, "Directories should come before files");
        Assert.True(alphaIndex < betaIndex, "Directories should come before files");
    }

    [Fact]
    public void Render_UsesCorrectBranchCharacters()
    {
        // Arrange
        var files = new List<FileEntry>
        {
            CreateEntry("first.txt"),
            CreateEntry("last.txt")
        };

        // Act
        var result = this._renderer.Render(files, "C:/test");

        // Assert
        Assert.Contains("├──", result);
        Assert.Contains("└──", result);
    }
}
