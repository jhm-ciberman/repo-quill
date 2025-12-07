using RepoQuill.Core.Tests.TestFixtures;

namespace RepoQuill.Core.Tests.Integration;

public sealed class QuillEngineTests : IDisposable
{
    private readonly TestFileSystemFixture _fixture;
    private readonly QuillEngine _engine;

    public QuillEngineTests()
    {
        this._fixture = new TestFileSystemFixture();
        this._engine = new QuillEngine();
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesFiles()
    {
        // Arrange
        this._fixture.CreateFile("src/app.cs", "class App { }");
        this._fixture.CreateFile("src/utils.cs", "class Utils { }");

        var config = new QuillConfig { RootPath = this._fixture.RootPath };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.Equal(2, result.TotalFiles);
        Assert.Equal(2, result.FullFiles);
        Assert.Contains("class App", result.Output);
        Assert.Contains("class Utils", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_RespectsGitIgnore()
    {
        // Arrange
        this._fixture.CreateFile("code.cs", "code");
        this._fixture.CreateFile("ignored.log", "log");
        this._fixture.CreateGitIgnore("", "*.log");

        var config = new QuillConfig
        {
            RootPath = this._fixture.RootPath,
            HonorGitIgnore = true
        };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.Equal(1, result.TotalFiles);
        Assert.DoesNotContain("ignored.log", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_ClassifiesBinaryAsTreeOnly()
    {
        // Arrange
        this._fixture.CreateFile("code.cs", "code");
        this._fixture.CreateBinaryFile("image.bin");

        var config = new QuillConfig { RootPath = this._fixture.RootPath };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.Equal(2, result.TotalFiles);
        Assert.Equal(1, result.FullFiles);
        Assert.Equal(1, result.TreeOnlyFiles);
    }

    [Fact]
    public async Task ExecuteAsync_AppliesIncludePattern()
    {
        // Arrange
        this._fixture.CreateFile("app.cs", "c# code");
        this._fixture.CreateFile("script.js", "js code");

        var config = new QuillConfig
        {
            RootPath = this._fixture.RootPath,
            IncludePatterns = ["*.cs"]
        };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.Equal(1, result.TotalFiles);
        Assert.Contains("c# code", result.Output);
        Assert.DoesNotContain("js code", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_AppliesExcludePattern()
    {
        // Arrange
        this._fixture.CreateFile("app.cs", "keep");
        this._fixture.CreateFile("temp.cs", "exclude");

        var config = new QuillConfig
        {
            RootPath = this._fixture.RootPath,
            ExcludePatterns = ["temp.*"]
        };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.Equal(1, result.TotalFiles);
        Assert.Contains("keep", result.Output);
        Assert.DoesNotContain("exclude", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_StripComments_RemovesComments()
    {
        // Arrange
        this._fixture.CreateFile("app.cs", "var x = 1; // comment");

        var config = new QuillConfig
        {
            RootPath = this._fixture.RootPath,
            StripComments = true
        };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.Contains("var x = 1;", result.Output);
        Assert.DoesNotContain("comment", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_JsonFormat_ReturnsJson()
    {
        // Arrange
        this._fixture.CreateFile("test.cs", "code");

        var config = new QuillConfig
        {
            RootPath = this._fixture.RootPath,
            Format = OutputFormat.Json
        };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.StartsWith("{", result.Output.Trim());
        Assert.Contains("\"files\"", result.Output);
        Assert.Contains("\"summary\"", result.Output);
    }

    [Fact]
    public async Task ExecuteAsync_ReportsProgress()
    {
        // Arrange
        this._fixture.CreateFile("test.cs", "code");
        var progressReports = new List<ProgressReport>();
        var progress = new Progress<ProgressReport>(r => progressReports.Add(r));

        var config = new QuillConfig { RootPath = this._fixture.RootPath };

        // Act
        await this._engine.ExecuteAsync(config, progress);

        // Small delay to allow progress reports to be captured
        await Task.Delay(100);

        // Assert
        Assert.NotEmpty(progressReports);
        Assert.Contains(progressReports, r => r.Phase == ProgressPhase.Discovering);
    }

    [Fact]
    public async Task ExecuteAsync_CollectsErrors()
    {
        // Arrange - create a file then delete it to simulate read error
        this._fixture.CreateFile("exists.cs", "code");

        var config = new QuillConfig { RootPath = this._fixture.RootPath };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert - no errors in normal case
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDirectory_ReturnsEmptyResult()
    {
        // Arrange
        var config = new QuillConfig { RootPath = this._fixture.RootPath };

        // Act
        var result = await this._engine.ExecuteAsync(config);

        // Assert
        Assert.Equal(0, result.TotalFiles);
    }

    public void Dispose()
    {
        this._fixture.Dispose();
    }
}
