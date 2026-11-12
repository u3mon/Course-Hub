using Application.DTOs.Categories;
using Application.DTOs.Courses;
using Application.DTOs.Students;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Models;

namespace Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly IStudentService _studentService;
    private readonly IEnrollmentService _enrollmentService;

    public AdminController(
        IAdminDashboardService dashboardService,
        ICourseService courseService,
        ICategoryService categoryService,
        IStudentService studentService,
        IEnrollmentService enrollmentService)
    {
        _dashboardService = dashboardService;
        _courseService = courseService;
        _categoryService = categoryService;
        _studentService = studentService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDashboardAsync(cancellationToken);
        return View(result.Value ?? new());
    }

    [HttpGet]
    public async Task<IActionResult> Courses([FromQuery] CourseFilterDto filter, CancellationToken cancellationToken)
    {
        var coursesResult = await _courseService.GetCatalogAsync(filter, null, cancellationToken);
        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);

        return View(new CatalogViewModel
        {
            Courses = coursesResult.Value ?? new(),
            Categories = categoriesResult.Value ?? [],
            Filter = filter
        });
    }

    [HttpGet]
    public async Task<IActionResult> CreateCourse(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return View(new CourseFormViewModel
        {
            Course = new CreateCourseDto { IsAvailable = true, Duration = "6 недель (36 часов)" },
            Categories = categories.Value ?? []
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseFormViewModel vm, CancellationToken cancellationToken)
    {
        await using var coverStream = vm.CoverImageFile?.OpenReadStream();
        var result = await _courseService.CreateAsync(
            vm.Course,
            User.GetUserId(),
            User.GetUserEmail(),
            coverStream,
            vm.CoverImageFile?.FileName,
            vm.CoverImageFile?.Length ?? 0,
            cancellationToken);

        vm.Categories = (await _categoryService.GetAllAsync(cancellationToken)).Value ?? [];
        return this.ToFormResult(result, vm, "Курс успешно создан.", nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id, CancellationToken cancellationToken)
    {
        var courseResult = await _courseService.GetByIdAsync(id, null, cancellationToken);
        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
        var c = courseResult.Value ?? new();

        return View(new CourseFormViewModel
        {
            Id = c.Id,
            Course = new CreateCourseDto
            {
                Title = c.Title,
                Description = c.Description ?? string.Empty,
                CategoryId = c.CategoryId,
                InstructorName = c.InstructorName,
                Duration = c.Duration,
                Price = c.Price,
                IsAvailable = c.IsAvailable,
                ImageUrl = c.ImageUrl,
                MaxStudents = c.MaxStudents
            },
            Categories = categoriesResult.Value ?? []
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int id, CourseFormViewModel vm, CancellationToken cancellationToken)
    {
        vm.Id = id;
        var updateDto = new UpdateCourseDto
        {
            Title = vm.Course.Title,
            Description = vm.Course.Description,
            CategoryId = vm.Course.CategoryId,
            InstructorName = vm.Course.InstructorName,
            Duration = vm.Course.Duration,
            Price = vm.Course.Price,
            IsAvailable = vm.Course.IsAvailable,
            ImageUrl = vm.Course.ImageUrl,
            MaxStudents = vm.Course.MaxStudents
        };

        await using var coverStream = vm.CoverImageFile?.OpenReadStream();
        var result = await _courseService.UpdateAsync(
            id,
            updateDto,
            User.GetUserId(),
            User.GetUserEmail(),
            coverStream,
            vm.CoverImageFile?.FileName,
            vm.CoverImageFile?.Length ?? 0,
            cancellationToken);

        vm.Categories = (await _categoryService.GetAllAsync(cancellationToken)).Value ?? [];
        return this.ToFormResult(result, vm, "Курс успешно обновлён.", nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken cancellationToken)
    {
        var result = await _courseService.DeleteAsync(id, User.GetUserId(), User.GetUserEmail(), cancellationToken);
        return this.ToRedirectResult(result, "Курс успешно удалён.", nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCourseAvailability(int id, CancellationToken cancellationToken)
    {
        var result = await _courseService.ToggleAvailabilityAsync(id, User.GetUserId(), User.GetUserEmail(), cancellationToken);
        return this.ToRedirectResult(result, "Доступность курса изменена.", nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> CourseStudents(int id, CancellationToken cancellationToken)
    {
        var courseResult = await _courseService.GetByIdAsync(id, null, cancellationToken);
        var enrolledResult = await _enrollmentService.GetCourseStudentsAsync(id, cancellationToken);
        var activeStudentsResult = await _studentService.GetAllActiveStudentsAsync(cancellationToken);

        var enrolledIds = (enrolledResult.Value ?? []).Select(e => e.UserId).ToHashSet();
        var availableStudents = (activeStudentsResult.Value ?? []).Where(s => !enrolledIds.Contains(s.Id)).ToList();

        return View(new AdminCourseStudentsViewModel
        {
            Course = courseResult.Value ?? new(),
            EnrolledStudents = enrolledResult.Value ?? [],
            AvailableStudents = availableStudents
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudentToCourse(int courseId, int studentId, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var result = await _enrollmentService.EnrollStudentAsync(
            studentId,
            courseId,
            isAdminOverride: true,
            adminId: User.GetUserId(),
            adminEmail: User.GetUserEmail(),
            cancellationToken: cancellationToken);

        return this.ToRedirectResult(result, "Студент успешно записан на курс.", nameof(CourseStudents), null, new { id = courseId }, returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudentFromCourse(int courseId, int studentId, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var result = await _enrollmentService.UnenrollStudentAsync(
            studentId,
            courseId,
            adminId: User.GetUserId(),
            adminEmail: User.GetUserEmail(),
            cancellationToken: cancellationToken);

        return this.ToRedirectResult(result, "Запись студента на курс удалена.", nameof(CourseStudents), null, new { id = courseId }, returnUrl, "Info");
    }

    [HttpGet]
    public async Task<IActionResult> Categories(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);
        return View(result.Value ?? []);
    }

    [HttpGet]
    public IActionResult CreateCategory() => View(new CreateCategoryDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(dto, User.GetUserId(), User.GetUserEmail(), cancellationToken);
        return this.ToFormResult(result, dto, "Категория успешно создана.", nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id, CancellationToken cancellationToken)
    {
        ViewData["CategoryId"] = id;
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        return View(new UpdateCategoryDto
        {
            Name = result.Value?.Name ?? string.Empty,
            Description = result.Value?.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        ViewData["CategoryId"] = id;
        var result = await _categoryService.UpdateAsync(id, dto, User.GetUserId(), User.GetUserEmail(), cancellationToken);
        return this.ToFormResult(result, dto, "Категория успешно обновлена.", nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(id, User.GetUserId(), User.GetUserEmail(), cancellationToken);
        return this.ToRedirectResult(result, "Категория успешно удалена.", nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> Students([FromQuery] StudentFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _studentService.GetStudentsPagedAsync(filter, cancellationToken);
        return View(new AdminStudentListViewModel
        {
            Students = result.Value ?? new(),
            Filter = filter
        });
    }

    [HttpGet]
    public async Task<IActionResult> StudentDetails(int id, CancellationToken cancellationToken)
    {
        var studentResult = await _studentService.GetStudentByIdAsync(id, cancellationToken);
        var allCoursesResult = await _courseService.GetAllAsync(cancellationToken);

        var enrolledIds = (studentResult.Value?.Enrollments ?? []).Select(e => e.CourseId).ToHashSet();
        var availableCourses = (allCoursesResult.Value ?? []).Where(c => !enrolledIds.Contains(c.Id)).ToList();

        return View(new AdminStudentDetailViewModel
        {
            Student = studentResult.Value ?? new(),
            AvailableCourses = availableCourses
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditStudent(int id, CancellationToken cancellationToken)
    {
        ViewData["StudentId"] = id;
        var studentResult = await _studentService.GetStudentByIdAsync(id, cancellationToken);
        var s = studentResult.Value ?? new();

        return View(new UpdateStudentDto
        {
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            IsActive = s.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStudent(int id, UpdateStudentDto dto, CancellationToken cancellationToken)
    {
        ViewData["StudentId"] = id;
        var result = await _studentService.UpdateStudentAsync(id, dto, User.GetUserId(), User.GetUserEmail(), cancellationToken);
        return this.ToFormResult(result, dto, "Данные студента успешно сохранены.", nameof(StudentDetails), routeValues: new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStudentStatus(int id, bool isActive, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var result = await _studentService.ChangeStudentStatusAsync(id, isActive, User.GetUserId(), User.GetUserEmail(), cancellationToken);
        return this.ToRedirectResult(result, "Статус аккаунта студента обновлён.", nameof(Students), null, null, returnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> Logs(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetActionLogsAsync(100, cancellationToken);
        return View(result.Value ?? []);
    }
}
