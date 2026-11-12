using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string InstructorName { get; set; } = null!;
    public string? Description { get; set; }
    public string Duration { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public int? MaxStudents { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public DateTime CreadAt
    {
        get => CreatedAt;
        set => CreatedAt = value;
    }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public List<Enrollment> Enrollments { get; set; } = [];
}
