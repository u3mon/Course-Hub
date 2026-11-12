using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByUserAndCourseAsync(int userId, int courseId, CancellationToken cancellationToken = default);
    Task<bool> IsEnrolledAsync(int userId, int courseId, CancellationToken cancellationToken = default);
    Task<int> GetCourseEnrollmentCountAsync(int courseId, CancellationToken cancellationToken = default);
    Task<List<Enrollment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<Enrollment>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);
    Task<List<Enrollment>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task DeleteAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
}
