using RepoQuill.Core.Classification;
using RepoQuill.Core.Discovery;
using RepoQuill.Core.Formatting;
using RepoQuill.Core.Loading;
using RepoQuill.Core.Models;
using RepoQuill.Core.Transforms;

namespace RepoQuill.Core;

/// <summary>
/// The main orchestrator for RepoQuill operations.
/// </summary>
public sealed class QuillEngine
{
    private readonly IFileDiscovery _discovery;
    private readonly IFileClassifier _classifier;
    private readonly IContentLoader _loader;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuillEngine"/> class with default implementations.
    /// </summary>
    public QuillEngine() : this(
        new FileScanner(),
        new PatternClassifier(),
        new FileReader())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuillEngine"/> class with custom implementations.
    /// </summary>
    /// <param name="discovery">The file discovery implementation.</param>
    /// <param name="classifier">The file classifier implementation.</param>
    /// <param name="loader">The content loader implementation.</param>
    public QuillEngine(
        IFileDiscovery discovery,
        IFileClassifier classifier,
        IContentLoader loader)
    {
        _discovery = discovery;
        _classifier = classifier;
        _loader = loader;
    }

    /// <summary>
    /// Executes a RepoQuill extraction operation.
    /// </summary>
    /// <param name="config">The configuration for the operation.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<QuillResult> ExecuteAsync(
        QuillConfig config,
        IProgress<ProgressReport>? progress = null,
        CancellationToken ct = default)
    {
        var errors = new List<FileError>();

        // Phase 1: Discovery
        ReportProgress(progress, ProgressPhase.Discovering, "", 0, -1);
        var discoveredFiles = new List<FileEntry>();

        await foreach (var file in _discovery.DiscoverAsync(config.RootPath, config.HonorGitIgnore, ct))
        {
            discoveredFiles.Add(file);
            ReportProgress(progress, ProgressPhase.Discovering, file.RelativePath, discoveredFiles.Count, -1);
        }

        // Phase 2: Classification
        ReportProgress(progress, ProgressPhase.Classifying, "", 0, discoveredFiles.Count);
        var classifiedFiles = new List<FileEntry>();

        for (int i = 0; i < discoveredFiles.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var classified = _classifier.Classify(discoveredFiles[i], config);
            classifiedFiles.Add(classified);
            ReportProgress(progress, ProgressPhase.Classifying, classified.RelativePath, i + 1, discoveredFiles.Count);
        }

        // Filter to non-excluded files
        var includedFiles = classifiedFiles
            .Where(f => f.State != FileState.Excluded)
            .OrderBy(f => f.RelativePath)
            .ToList();

        var fullFiles = includedFiles.Where(f => f.State == FileState.Full).ToList();

        // Phase 3: Loading
        ReportProgress(progress, ProgressPhase.Loading, "", 0, fullFiles.Count);
        var contents = new List<FileContent>();

        for (int i = 0; i < fullFiles.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var file = fullFiles[i];
            ReportProgress(progress, ProgressPhase.Loading, file.RelativePath, i + 1, fullFiles.Count);

            var result = await _loader.LoadAsync(file, ct);
            if (result.IsSuccess)
            {
                contents.Add(result.Value!);
            }
            else
            {
                errors.Add(result.Error!);
            }
        }

        // Phase 4: Transformations
        if (config.StripComments || config.NormalizeWhitespace)
        {
            ReportProgress(progress, ProgressPhase.Transforming, "", 0, contents.Count);
            var transforms = BuildTransforms(config);

            for (int i = 0; i < contents.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var content = contents[i];
                ReportProgress(progress, ProgressPhase.Transforming, content.Entry.RelativePath, i + 1, contents.Count);

                foreach (var transform in transforms)
                {
                    content = transform.Transform(content);
                }
                contents[i] = content;
            }
        }

        // Phase 5: Formatting
        ReportProgress(progress, ProgressPhase.Formatting, "", 0, 1);
        var formatter = CreateFormatter(config);
        var output = formatter.Format(includedFiles, contents);
        ReportProgress(progress, ProgressPhase.Formatting, "", 1, 1);

        return new QuillResult
        {
            Output = output,
            TotalFiles = includedFiles.Count,
            FullFiles = fullFiles.Count,
            TreeOnlyFiles = includedFiles.Count - fullFiles.Count,
            TotalSizeBytes = includedFiles.Sum(f => f.SizeBytes),
            Files = includedFiles,
            Errors = errors
        };
    }

    private List<IContentTransform> BuildTransforms(QuillConfig config)
    {
        var transforms = new List<IContentTransform>();

        if (config.StripComments)
            transforms.Add(new CommentStripper());

        if (config.NormalizeWhitespace)
            transforms.Add(new WhitespaceNormalizer());

        return transforms;
    }

    private IOutputFormatter CreateFormatter(QuillConfig config)
    {
        return config.Format switch
        {
            OutputFormat.Json => new JsonFormatter(),
            _ => new PlainTextFormatter()
        };
    }

    private void ReportProgress(
        IProgress<ProgressReport>? progress,
        ProgressPhase phase,
        string currentFile,
        int processedCount,
        int totalCount)
    {
        progress?.Report(new ProgressReport
        {
            Phase = phase,
            CurrentFile = currentFile,
            ProcessedCount = processedCount,
            TotalCount = totalCount
        });
    }
}
