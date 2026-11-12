using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Categories;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CoursesCount { get; set; }
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Название категории обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название категории должно содержать от 2 до 100 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
}

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Название категории обязательно")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Название категории должно содержать от 2 до 100 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
}
