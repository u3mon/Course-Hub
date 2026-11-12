using Application.Common;

namespace Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<Result<string>> SaveCourseCoverAsync(
        Stream? fileStream,
        string? fileName,
        long fileLength,
        CancellationToken cancellationToken = default);
}
