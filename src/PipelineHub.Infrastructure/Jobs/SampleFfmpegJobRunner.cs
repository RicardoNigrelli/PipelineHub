using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PipelineHub.Application.Jobs;
using PipelineHub.Domain;

namespace PipelineHub.Infrastructure.Jobs;

/// <summary>
/// Public, self-contained job: resizes the repo's committed sample.mp4 with ffmpeg.
/// Ships in the public repo so a fresh clone works out of the box, with no dependency on
/// video-lab or any private client media. Real video-lab/reel-lab runners are registered
/// separately, gated behind local (gitignored) configuration.
/// </summary>
public sealed class SampleFfmpegJobRunner : IJobRunner
{
    private readonly ILogger<SampleFfmpegJobRunner> _logger;
    private readonly string _repoRoot;

    public JobType Handles => JobType.SampleFfmpegTranscode;

    public SampleFfmpegJobRunner(ILogger<SampleFfmpegJobRunner> logger, string repoRoot)
    {
        _logger = logger;
        _repoRoot = repoRoot;
    }

    public async Task<JobRunOutcome> RunAsync(IReadOnlyDictionary<string, string> parameters, CancellationToken cancellationToken)
    {
        var inputPath = Path.Combine(_repoRoot, "assets", "sample.mp4");
        if (!File.Exists(inputPath))
        {
            return new JobRunOutcome(false, null, $"Sample input not found at '{inputPath}'.");
        }

        var width = parameters.GetValueOrDefault("width", "320");
        var outputDir = Path.Combine(Path.GetTempPath(), "pipelinehub-jobs");
        Directory.CreateDirectory(outputDir);
        var outputPath = Path.Combine(outputDir, $"sample-{Guid.NewGuid():N}.mp4");

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            ArgumentList = { "-y", "-i", inputPath, "-vf", $"scale={width}:-1", outputPath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = new Process { StartInfo = startInfo };
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) _logger.LogDebug("ffmpeg stdout: {Line}", e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) _logger.LogDebug("ffmpeg stderr: {Line}", e.Data); };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return new JobRunOutcome(false, null, $"Failed to start ffmpeg: {ex.Message}");
        }

        if (process.ExitCode != 0)
        {
            return new JobRunOutcome(false, null, $"ffmpeg exited with code {process.ExitCode}.");
        }

        return File.Exists(outputPath)
            ? new JobRunOutcome(true, outputPath, null)
            : new JobRunOutcome(false, null, "ffmpeg reported success but the expected output file is missing.");
    }
}
