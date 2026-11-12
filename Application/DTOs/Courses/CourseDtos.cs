using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Courses;

public class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ShortDescription =>
        string.IsNullOrWhiteSpace(Description)
            ? "Описание курса отсутствует."
            : (Description.Length > 130 ? Description[..130] + "..." : Description);
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public int? MaxStudents { get; set; }
    public int EnrolledCount { get; set; }
    public bool IsFull => MaxStudents.HasValue && EnrolledCount >= MaxStudents.Value;
    public bool CanEnroll => IsAvailable && !IsFull;
    public bool IsEnrolled { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CourseFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsAvailable { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 6;
}

public class CreateCourseDto
{
    [Required(ErrorMessage = "Название курса обязательно")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Название курса должно быть от 3 до 150 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Описание курса обязательно")]
    [StringLength(3000, MinimumLength = 10, ErrorMessage = "Описание должно быть от 10 до 3000 символов")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите категорию")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите корректную категорию")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Укажите имя преподавателя")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя преподавателя должно быть от 2 до 100 символов")]
    public string InstructorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите продолжительность курса")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Продолжительность должна быть от 2 до 60 символов (например: '8 недель' или '36 часов')")]
    public string Duration { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите стоимость курса")]
    [Range(0, 1000000, ErrorMessage = "Стоимость должна быть от 0 до 1 000 000")]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    [StringLength(500, ErrorMessage = "Ссылка на изображение не должна превышать 500 символов")]
    public string? ImageUrl { get; set; }

    [Range(1, 10000, ErrorMessage = "Лимит студентов должен быть от 1 до 10 000")]
    public int? MaxStudents { get; set; }
}

public class UpdateCourseDto
{
    [Required(ErrorMessage = "Название курса обязательно")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Название курса должно быть от 3 до 150 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Описание курса обязательно")]
    [StringLength(3000, MinimumLength = 10, ErrorMessage = "Описание должно быть от 10 до 3000 символов")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите категорию")]
    [Range(1, int.MaxValue, ErrorMessage = "Выберите корректную категорию")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Укажите имя преподавателя")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя преподавателя должно быть от 2 до 100 символов")]
    public string InstructorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите продолжительность курса")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Продолжительность должна быть от 2 до 60 символов")]
    public string Duration { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите стоимость курса")]
    [Range(0, 1000000, ErrorMessage = "Стоимость должна быть от 0 до 1 000 000")]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    [StringLength(500, ErrorMessage = "Ссылка на изображение не должна превышать 500 символов")]
    public string? ImageUrl { get; set; }

    [Range(1, 10000, ErrorMessage = "Лимит студентов должен быть от 1 до 10 000")]
    public int? MaxStudents { get; set; }
}
