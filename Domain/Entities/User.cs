using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public int Id {get;set;}
    public string FirstName {get;set;} = null!;
    public string LastName {get;set;} = null!;
    public string Email {get;set;} = null!;
    public string PasswordHash {get;set;} = null!;
    public UserRole Role {get;set;} = UserRole.Student;
    public bool IsActive {get;set;} = true;
    public DateTime CreatedAt {get;set;} = DateTime.UtcNow;

    public List<Enrollment> Enrollments {get;set;} = [];
}
