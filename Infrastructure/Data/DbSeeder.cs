using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await context.Database.MigrateAsync();

        var adminEmail = (configuration["SeedUsers:AdminEmail"] ?? "admin@coursehub.local").Trim().ToLowerInvariant();
        var adminPassword = configuration["SeedUsers:AdminPassword"] ?? "Admin123!";
        var adminFirstName = configuration["SeedUsers:AdminFirstName"] ?? "usmon";
        var adminLastName = configuration["SeedUsers:AdminLastName"] ?? "Администратор";

        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin || u.Email.ToLower() == adminEmail);
        if (existingAdmin is null)
        {
            var admin = new User
            {
                FirstName = adminFirstName,
                LastName = adminLastName,
                Email = adminEmail,
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = passwordHasher.HashPassword(admin, adminPassword);
            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded Admin account: {Email}", adminEmail);
        }
        else
        {
            existingAdmin.FirstName = adminFirstName;
            existingAdmin.LastName = adminLastName;
            existingAdmin.Email = adminEmail;
            existingAdmin.PasswordHash = passwordHasher.HashPassword(existingAdmin, adminPassword);
            await context.SaveChangesAsync();
        }

        var studentEmail = (configuration["SeedUsers:StudentEmail"] ?? "student@coursehub.local").Trim().ToLowerInvariant();
        var studentPassword = configuration["SeedUsers:StudentPassword"] ?? "Student123!";
        var studentFirstName = configuration["SeedUsers:StudentFirstName"] ?? "Student's firsname";
        var studentLastName = configuration["SeedUsers:StudentLastName"] ?? "Student's lastname";

        User? demoStudent = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == studentEmail);
        if (demoStudent is null)
        {
            demoStudent = new User
            {
                FirstName = studentFirstName,
                LastName = studentLastName,
                Email = studentEmail,
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            };
            demoStudent.PasswordHash = passwordHasher.HashPassword(demoStudent, studentPassword);

            var secondStudent = new User
            {
                FirstName = "Student's Firstname",
                LastName = "Student;s Lastname",
                Email = "aaaa@coursehub.local",
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            };
            secondStudent.PasswordHash = passwordHasher.HashPassword(secondStudent, studentPassword);

            await context.Users.AddRangeAsync(demoStudent, secondStudent);
            await context.SaveChangesAsync();
        }
        else
        {
            demoStudent.FirstName = studentFirstName;
            demoStudent.LastName = studentLastName;
            demoStudent.PasswordHash = passwordHasher.HashPassword(demoStudent, studentPassword);
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new()
                {
                    Name = "Backend-разработка",
                    Description = "Проектирование серверных приложений, микросервисов и высоконагруженных API на C# и .NET."
                },
                new()
                {
                    Name = "Frontend и Web UI",
                    Description = "Современная вёрстка, интерактивные интерфейсы, TypeScript и компонентная архитектура."
                },
                new()
                {
                    Name = "Базы данных и Архитектура",
                    Description = "PostgreSQL, оптимизация SQL-запросов, Clean Architecture, DDD и паттерны проектирования."
                },
                new()
                {
                    Name = "DevOps и Облака",
                    Description = "Контейнеризация Docker, CI/CD пайплайны, автоматизация развёртывания и мониторинг."
                },
                new()
                {
                    Name = "Data Science и AI",
                    Description = "Анализ данных, машинное обучение и интеграция языковых моделей в продукты."
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        var allCategories = await context.Categories.OrderBy(c => c.Id).ToListAsync();
        var backendCat = allCategories[0];
        var frontendCat = allCategories.ElementAtOrDefault(1) ?? backendCat;
        var dbCat = allCategories.ElementAtOrDefault(2) ?? backendCat;
        var devopsCat = allCategories.ElementAtOrDefault(3) ?? backendCat;

        var seedCourses = new List<Course>
        {
            new()
            {
                Title = "ASP.NET Core MVC и Clean Architecture на практике",
                InstructorName = "Muhammad Nematov",
                Description = "Полное руководство по созданию корпоративных веб-приложений на платформе .NET. Вы изучите слоистую архитектуру, паттерны Repository и Result, безопасную аутентификацию с JWT, работу с Entity Framework Core и PostgreSQL.",
                CategoryId = backendCat.Id,
                Duration = "8 недель (48 часов)",
                Price = 14900,
                IsAvailable = true,
                MaxStudents = 30,
                ImageUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=900&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Title = "Проектирование БД и оптимизация PostgreSQL",
                InstructorName = "Behruz Saidzoda",
                Description = "Глубокое погружение в реляционные базы данных: нормализация, индексы B-Tree и GIN, транзакции, уровни изоляции, разбор планов выполнения EXPLAIN ANALYZE и эффективная работа с EF Core.",
                CategoryId = dbCat.Id,
                Duration = "6 недель (36 часов)",
                Price = 11500,
                IsAvailable = true,
                MaxStudents = 25,
                ImageUrl = "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?auto=format&fit=crop&w=900&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Title = "Современный UI/UX и адаптивная вёрстка",
                InstructorName = "Илья Лебедев",
                Description = "Практический курс по проектированию чистых минималистичных веб-интерфейсов: модульные сетки, типографика, дизайн-системы, CSS Grid/Flexbox и доступность пользовательских форм.",
                CategoryId = frontendCat.Id,
                Duration = "5 недель (30 часов)",
                Price = 8900,
                IsAvailable = true,
                MaxStudents = 40,
                ImageUrl = "https://images.unsplash.com/photo-1507238691740-187a5b1d37b8?auto=format&fit=crop&w=900&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                Title = "Docker и CI/CD для .NET-разработчиков",
                InstructorName = "Akramjon Kholiqov",
                Description = "Упаковка многослойных .NET-приложений в Docker-контейнеры, настройка docker-compose с PostgreSQL, автоматизация сборки и тестирования в GitHub Actions и GitLab CI.",
                CategoryId = devopsCat.Id,
                Duration = "4 недели (24 часа)",
                Price = 9900,
                IsAvailable = true,
                MaxStudents = 20,
                ImageUrl = "https://images.unsplash.com/photo-1618401471353-b98afee0b2eb?auto=format&fit=crop&w=900&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Title = "Паттерны проектирования и SOLID в C#",
                InstructorName = "Abubakr Okhundodoev",
                Description = "Разбор принципов SOLID, чистого кода, Dependency Injection, модульного тестирования и рефакторинга реальных бизнес-сервисов без дублирования логики.",
                CategoryId = dbCat.Id,
                Duration = "6 недель (32 часа)",
                Price = 0,
                IsAvailable = true,
                MaxStudents = 100,
                ImageUrl = "https://images.unsplash.com/photo-1516116216624-53e697fedbea?auto=format&fit=crop&w=900&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Title = "Микросервисная архитектура Highload-систем",
                InstructorName = "Hojiakbar Sharifov",
                Description = "Продвинутый курс по асинхронному взаимодействию сервисов через брокеры сообщений, распределённым транзакциям Saga, кэшированию Redis и трассировке OpenTelemetry.",
                CategoryId = backendCat.Id,
                Duration = "10 недель (60 часов)",
                Price = 19500,
                IsAvailable = false,
                MaxStudents = 15,
                ImageUrl = "https://images.unsplash.com/photo-1451187580459-43490279c0fa?auto=format&fit=crop&w=900&q=80",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        if (!await context.Courses.AnyAsync())
        {
            await context.Courses.AddRangeAsync(seedCourses);
            await context.SaveChangesAsync();

            if (demoStudent is not null)
            {
                await context.Enrollments.AddAsync(new Enrollment
                {
                    UserId = demoStudent.Id,
                    CourseId = seedCourses[0].Id,
                    EnrollDate = DateTime.UtcNow.AddDays(-4)
                });
                await context.SaveChangesAsync();
            }
        }
        else
        {
            var existingCourses = await context.Courses.ToListAsync();
            foreach (var seedCourse in seedCourses)
            {
                var dbCourse = existingCourses.FirstOrDefault(c => c.Title == seedCourse.Title);
                if (dbCourse is not null)
                {
                    dbCourse.InstructorName = seedCourse.InstructorName;
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
