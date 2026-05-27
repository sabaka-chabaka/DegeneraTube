namespace DegeneraTube.Infrastructure.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream stream, string folder, string fileName, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<Stream> GetAsync(string path, CancellationToken ct = default);
    bool Exists(string path);
    string GetFullPath(string relativePath);
}