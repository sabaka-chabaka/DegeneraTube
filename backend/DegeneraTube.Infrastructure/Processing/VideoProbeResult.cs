using System.Text.Json;

namespace DegeneraTube.Infrastructure.Processing;

public record VideoProbeResult(int Width, int Height, int DurationSeconds)
{
    public static VideoProbeResult Parse(string ffprobeJson)
    {
        using var doc = JsonDocument.Parse(ffprobeJson);
        var streams = doc.RootElement.GetProperty("streams");

        int width = 0, height = 0, duration = 0;

        foreach (var stream in streams.EnumerateArray())
        {
            if (!stream.TryGetProperty("codec_type", out var codecType))
                continue;

            if (codecType.GetString() != "video")
                continue;

            width = stream.GetProperty("width").GetInt32();
            height = stream.GetProperty("height").GetInt32();

            if (stream.TryGetProperty("duration", out var dur) &&
                double.TryParse(dur.GetString(), out var d))
            {
                duration = (int)d;
            }

            break;
        }

        return new VideoProbeResult(width, height, duration);
    }
}