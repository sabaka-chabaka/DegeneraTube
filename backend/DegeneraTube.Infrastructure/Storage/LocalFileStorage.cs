namespace DegeneraTube.Infrastructure.Storage;

public class LocalFileStorage(string basePath) : IFileStorage
{
    public async Task<string> SaveAsync(
        Stream stream, string folder, string fileName, CancellationToken ct = default)
    {
        var dir = Path.Combine(basePath, folder);
        Directory.CreateDirectory(dir);

        var relativePath = Path.Combine(folder, fileName);
        var fullPath = Path.Combine(basePath, relativePath);

        await using var file = File.Create(fullPath);
        await stream.CopyToAsync(file, ct);

        return relativePath;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(basePath, path);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<Stream> ReadAsync(string path, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(basePath, path);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public bool Exists(string path) =>
        File.Exists(Path.Combine(basePath, path));

    public string GetFullPath(string relativePath) =>
        Path.Combine(basePath, relativePath);
}