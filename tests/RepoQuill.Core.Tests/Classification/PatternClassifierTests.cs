using RepoQuill.Core.Classification;
using RepoQuill.Core.Models;

namespace RepoQuill.Core.Tests.Classification;

public sealed class PatternClassifierTests
{
    private readonly PatternClassifier _classifier = new();

    private static FileEntry CreateEntry(string relativePath) => new()
    {
        AbsolutePath = $"C:/test/{relativePath}",
        RelativePath = relativePath,
        SizeBytes = 100,
        LastModified = DateTime.UtcNow
    };

    [Fact]
    public void Classify_ExcludePattern_ReturnsExcluded()
    {
        // Arrange
        var entry = CreateEntry("logs/app.log");
        var config = new QuillConfig
        {
            RootPath = "C:/test",
            ExcludePatterns = ["*.log"]
        };

        // Act
        var result = this._classifier.Classify(entry, config);

        // Assert
        Assert.Equal(FileState.Excluded, result.State);
    }

    [Fact]
    public void Classify_TreeOnlyPattern_ReturnsTreeOnly()
    {
        // Arrange
        var entry = CreateEntry("assets/logo.png");
        var config = new QuillConfig
        {
            RootPath = "C:/test",
            TreeOnlyPatterns = ["*.png"]
        };

        // Act
        var result = this._classifier.Classify(entry, config);

        // Assert
        Assert.Equal(FileState.TreeOnly, result.State);
    }

    [Fact]
    public void Classify_IncludePatternMatches_ReturnsFull()
    {
        // Arrange
        var entry = CreateEntry("src/app.cs");
        var config = new QuillConfig
        {
            RootPath = "C:/test",
            IncludePatterns = ["*.cs"]
        };

        // Act
        var result = this._classifier.Classify(entry, config);

        // Assert
        Assert.Equal(FileState.Full, result.State);
    }

    [Fact]
    public void Classify_IncludePatternNoMatch_ReturnsExcluded()
    {
        // Arrange
        var entry = CreateEntry("readme.md");
        var config = new QuillConfig
        {
            RootPath = "C:/test",
            IncludePatterns = ["*.cs"]
        };

        // Act
        var result = this._classifier.Classify(entry, config);

        // Assert
        Assert.Equal(FileState.Excluded, result.State);
    }

    [Fact]
    public void Classify_NoPatterns_ReturnsFull()
    {
        // Arrange
        var entry = CreateEntry("src/app.cs");
        var config = new QuillConfig { RootPath = "C:/test" };

        // Act
        var result = this._classifier.Classify(entry, config);

        // Assert
        Assert.Equal(FileState.Full, result.State);
    }

    [Fact]
    public void Classify_ExcludeTakesPrecedence()
    {
        // Arrange
        var entry = CreateEntry("src/temp.cs");
        var config = new QuillConfig
        {
            RootPath = "C:/test",
            IncludePatterns = ["*.cs"],
            ExcludePatterns = ["**/temp.*"]
        };

        // Act
        var result = this._classifier.Classify(entry, config);

        // Assert
        Assert.Equal(FileState.Excluded, result.State);
    }

    [Fact]
    public void Classify_GlobStarPattern_MatchesNestedPaths()
    {
        // Arrange
        var entry = CreateEntry("src/deep/nested/file.cs");
        var config = new QuillConfig
        {
            RootPath = "C:/test",
            IncludePatterns = ["**/*.cs"]
        };

        // Act
        var result = this._classifier.Classify(entry, config);

        // Assert
        Assert.Equal(FileState.Full, result.State);
    }
}
