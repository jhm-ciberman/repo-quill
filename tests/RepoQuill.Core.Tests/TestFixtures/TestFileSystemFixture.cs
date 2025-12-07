namespace RepoQuill.Core.Tests.TestFixtures;

public sealed class TestFileSystemFixture : IDisposable
{
    public string RootPath { get; }

    public TestFileSystemFixture()
    {
        this.RootPath = Path.Combine(Path.GetTempPath(), $"repoquill-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(this.RootPath);
    }

    public void CreateFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(this.RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content);
    }

    public void CreateGitIgnore(string relativePath, params string[] patterns)
    {
        var content = string.Join(Environment.NewLine, patterns);
        this.CreateFile(Path.Combine(relativePath, ".gitignore"), content);
    }

    public void CreateBinaryFile(string relativePath)
    {
        var fullPath = Path.Combine(this.RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write some bytes including null bytes to make it binary
        File.WriteAllBytes(fullPath, new byte[] { 0x00, 0x01, 0x02, 0x00, 0xFF });
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(this.RootPath))
            {
                Directory.Delete(this.RootPath, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }
}
