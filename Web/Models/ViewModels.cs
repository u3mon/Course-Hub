using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Categories;
using Application.DTOs.Courses;
using Application.DTOs.Enrollments;
using Application.DTOs.Students;

namespace Web.Models;

public class CatalogViewModel
{
    public PagedResult<CourseDto> Courses { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = [];
    public CourseFilterDto Filter { get; set; } = new();
}

public class ProfileViewModel
{
    public UserDto User { get; set; } = new();
    public List<EnrollmentDto> Enrollments { get; set; } = [];
}

public class CourseFormViewModel
{
    public int? Id { get; set; }
    public CreateCourseDto Course { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = [];
    public IFormFile? CoverImageFile { get; set; }
}

public class AdminCourseStudentsViewModel
{
    public CourseDto Course { get; set; } = new();
    public List<EnrollmentDto> EnrolledStudents { get; set; } = [];
    public List<UserDto> AvailableStudents { get; set; } = [];
}

public class AdminStudentListViewModel
{
    public PagedResult<UserDto> Students { get; set; } = new();
    public StudentFilterDto Filter { get; set; } = new();
}

public class AdminStudentDetailViewModel
{
    public StudentDetailDto Student { get; set; } = new();
    public List<CourseDto> AvailableCourses { get; set; } = [];
}
