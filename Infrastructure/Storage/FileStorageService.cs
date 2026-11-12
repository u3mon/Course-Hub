using Application.Common;
using Application.Interfaces.Services;

namespace Infrastructure.Storage;

public class FileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;
    private readonly string _webRootPath;

    public FileStorageService(string webRootPath)
    {
        _webRootPath = webRootPath;
    }

    public async Task<Result<string>> SaveCourseCoverAsync(
        Stream? fileStream,
        string? fileName,
        long fileLength,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null || string.IsNullOrWhiteSpace(fileName) || fileLength <= 0)
        {
            return Result<string>.Success(string.Empty);
        }

        if (fileLength > MaxFileSize)
        {
            return Result<string>.Failure("Размер изображения не должен превышать 5 МБ.", ErrorType.Validation);
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            return Result<string>.Failure("Допустимы только изображения форматов .jpg, .jpeg, .png, .webp.", ErrorType.Validation);
        }

        var uploadsFolder = Path.Combine(_webRootPath, "uploads", "courses");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        return Result<string>.Success($"/uploads/courses/{uniqueFileName}");
    }
}
