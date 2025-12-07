using RepoQuill.Core.Models;
using RepoQuill.Core.Transforms;

namespace RepoQuill.Core.Tests.Transforms;

public sealed class CommentStripperTests
{
    private readonly CommentStripper _stripper = new();

    private static FileContent CreateContent(string relativePath, string content) => new(
        new FileEntry
        {
            AbsolutePath = $"C:/test/{relativePath}",
            RelativePath = relativePath,
            SizeBytes = content.Length,
            LastModified = DateTime.UtcNow
        },
        content);

    [Fact]
    public void Transform_CSharp_RemovesSingleLineComments()
    {
        // Arrange
        var content = CreateContent("test.cs", "var x = 1; // comment\nvar y = 2;");

        // Act
        var result = this._stripper.Transform(content);

        // Assert
        Assert.DoesNotContain("comment", result.Content);
        Assert.Contains("var x = 1;", result.Content);
        Assert.Contains("var y = 2;", result.Content);
    }

    [Fact]
    public void Transform_CSharp_RemovesMultiLineComments()
    {
        // Arrange
        var content = CreateContent("test.cs", "var x = 1; /* block\ncomment */ var y = 2;");

        // Act
        var result = this._stripper.Transform(content);

        // Assert
        Assert.DoesNotContain("block", result.Content);
        Assert.DoesNotContain("comment", result.Content);
        Assert.Contains("var x = 1;", result.Content);
        Assert.Contains("var y = 2;", result.Content);
    }

    [Fact]
    public void Transform_Python_RemovesHashComments()
    {
        // Arrange
        var content = CreateContent("test.py", "x = 1 # comment\ny = 2");

        // Act
        var result = this._stripper.Transform(content);

        // Assert
        Assert.DoesNotContain("comment", result.Content);
        Assert.Contains("x = 1", result.Content);
        Assert.Contains("y = 2", result.Content);
    }

    [Fact]
    public void Transform_Python_PreservesShebang()
    {
        // Arrange
        var content = CreateContent("test.py", "#!/usr/bin/env python\nx = 1");

        // Act
        var result = this._stripper.Transform(content);

        // Assert
        Assert.Contains("#!/usr/bin/env python", result.Content);
    }

    [Fact]
    public void Transform_Html_RemovesXmlComments()
    {
        // Arrange
        var content = CreateContent("test.html", "<div><!-- comment --><span>text</span></div>");

        // Act
        var result = this._stripper.Transform(content);

        // Assert
        Assert.DoesNotContain("comment", result.Content);
        Assert.Contains("<div>", result.Content);
        Assert.Contains("<span>text</span>", result.Content);
    }

    [Fact]
    public void Transform_UnknownExtension_ReturnsUnchanged()
    {
        // Arrange
        var originalContent = "some content // with slashes";
        var content = CreateContent("test.xyz", originalContent);

        // Act
        var result = this._stripper.Transform(content);

        // Assert
        Assert.Equal(originalContent, result.Content);
    }

    [Fact]
    public void Transform_PreservesUrls()
    {
        // Arrange
        var content = CreateContent("test.cs", "var url = \"https://example.com\";");

        // Act
        var result = this._stripper.Transform(content);

        // Assert
        Assert.Contains("https://example.com", result.Content);
    }
}
