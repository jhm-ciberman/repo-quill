using System.Text.Json;
using System.Text.Json.Serialization;
using RepoQuill.Core.Models;
using RepoQuill.Core.Tree;

namespace RepoQuill.Core.Formatting;

/// <summary>
/// Formats output as structured JSON.
/// </summary>
public sealed class JsonFormatter : IOutputFormatter
{
    private readonly ITreeRenderer _treeRenderer;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFormatter"/> class with the default ASCII tree renderer.
    /// </summary>
    public JsonFormatter() : this(new AsciiTreeRenderer())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFormatter"/> class with a custom tree renderer.
    /// </summary>
    /// <param name="treeRenderer">The tree renderer to use for generating the directory tree.</param>
    public JsonFormatter(ITreeRenderer treeRenderer)
    {
        _treeRenderer = treeRenderer;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc/>
    public string Format(IReadOnlyList<FileEntry> allFiles, IReadOnlyList<FileContent> contents)
    {
        var tree = _treeRenderer.Render(allFiles, string.Empty);

        // Build content lookup
        var contentMap = contents.ToDictionary(c => c.Entry.RelativePath, c => c.Content);

        var output = new JsonOutput
        {
            Tree = tree,
            Files = allFiles.Select(f => new JsonFileEntry
            {
                Path = f.RelativePath,
                State = f.State.ToString(),
                SizeBytes = f.SizeBytes,
                Content = contentMap.GetValueOrDefault(f.RelativePath)
            }).ToList(),
            Summary = new JsonSummary
            {
                TotalFiles = allFiles.Count,
                FullFiles = allFiles.Count(f => f.State == FileState.Full),
                TreeOnlyFiles = allFiles.Count(f => f.State == FileState.TreeOnly),
                TotalSizeBytes = allFiles.Sum(f => f.SizeBytes)
            }
        };

        return JsonSerializer.Serialize(output, _jsonOptions);
    }

    private class JsonOutput
    {
        public string Tree { get; set; } = string.Empty;
        public List<JsonFileEntry> Files { get; set; } = new();
        public JsonSummary Summary { get; set; } = new();
    }

    private class JsonFileEntry
    {
        public string Path { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string? Content { get; set; }
    }

    private class JsonSummary
    {
        public int TotalFiles { get; set; }
        public int FullFiles { get; set; }
        public int TreeOnlyFiles { get; set; }
        public long TotalSizeBytes { get; set; }
    }
}
