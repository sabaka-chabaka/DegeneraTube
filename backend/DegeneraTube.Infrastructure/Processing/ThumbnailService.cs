using System.Diagnostics;

namespace DegeneraTube.Infrastructure.Processing;

public class ThumbnailService(string ffmpegPath)
{
    public async Task<string> ExtractAsync(
        string videoPath, string outputDir, int atSecond = 0, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputDir);

        var fileName = $"thumb_{atSecond}s.jpg";
        var outputPath = Path.Combine(outputDir, fileName);

        var args = $"-ss {atSecond} -i \"{videoPath}\" " +
                   $"-vframes 1 -q:v 2 -vf scale=640:-2 " +
                   $"\"{outputPath}\"";

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0 || !File.Exists(outputPath))
            throw new InvalidOperationException($"Thumbnail extraction failed at {atSecond}s.");

        return outputPath;
    }

    public async Task<string> ExtractBestAsync(
        string videoPath, string outputDir, int durationSeconds, CancellationToken ct = default)
    {
        var candidates = new[] { 0, durationSeconds / 4, durationSeconds / 2 }
            .Where(s => s < durationSeconds)
            .ToArray();

        var tasks = candidates.Select(s => ExtractAsync(videoPath, outputDir, s, ct));
        var paths = await Task.WhenAll(tasks);

        return paths.First();
    }
}