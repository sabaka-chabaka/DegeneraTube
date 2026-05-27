using System.Diagnostics;

namespace DegeneraTube.Infrastructure.Processing;

public class FfmpegService(string ffmpegPath)
{
    private static readonly int[] Resolutions = [360, 720, 1080];

    public async Task<HlsResult> TranscodeToHlsAsync(
        string inputPath, string outputDir, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outputDir);

        var successfulResolutions = new List<int>();

        foreach (var height in Resolutions)
        {
            var probe = await ProbeAsync(inputPath, ct);
            if (probe.Height < height && height != Resolutions[0])
                continue;

            var resDir = Path.Combine(outputDir, $"{height}p");
            Directory.CreateDirectory(resDir);

            var args = BuildHlsArgs(inputPath, resDir, height);
            await RunFfmpegAsync(args, ct);
            successfulResolutions.Add(height);
        }

        var masterPath = Path.Combine(outputDir, "master.m3u8");
        await WriteMasterPlaylistAsync(masterPath, outputDir, successfulResolutions);

        return new HlsResult(masterPath, successfulResolutions);
    }

    public async Task<VideoProbeResult> ProbeAsync(string inputPath, CancellationToken ct = default)
    {
        var args = $"-v quiet -print_format json -show_streams \"{inputPath}\"";
        var output = await RunFfprobeAsync(args, ct);
        return VideoProbeResult.Parse(output);
    }

    private static string BuildHlsArgs(string input, string outDir, int height)
    {
        var playlist = Path.Combine(outDir, "index.m3u8");
        var segment = Path.Combine(outDir, "seg%03d.ts");

        return $"-i \"{input}\" " +
               $"-vf scale=-2:{height} " +
               $"-c:v libx264 -preset fast -crf 23 " +
               $"-c:a aac -b:a 128k " +
               $"-hls_time 6 -hls_list_size 0 " +
               $"-hls_segment_filename \"{segment}\" " +
               $"\"{playlist}\"";
    }

    private static async Task WriteMasterPlaylistAsync(
        string masterPath, string outputDir, List<int> resolutions)
    {
        var lines = new List<string> { "#EXTM3U" };

        var bandwidthMap = new Dictionary<int, int>
        {
            [360] = 800_000,
            [720] = 2_800_000,
            [1080] = 5_000_000
        };

        foreach (var res in resolutions)
        {
            lines.Add($"#EXT-X-STREAM-INF:BANDWIDTH={bandwidthMap[res]},RESOLUTION=x{res}");
            lines.Add($"{res}p/index.m3u8");
        }

        await File.WriteAllLinesAsync(masterPath, lines);
    }

    private async Task RunFfmpegAsync(string args, CancellationToken ct)
    {
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

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync(ct);
            throw new InvalidOperationException($"FFmpeg failed: {error}");
        }
    }

    private async Task<string> RunFfprobeAsync(string args, CancellationToken ct)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(Path.GetDirectoryName(ffmpegPath)!, "ffprobe"),
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        return output;
    }
}

public record HlsResult(string MasterPlaylistPath, List<int> Resolutions);