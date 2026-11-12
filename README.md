# CourseHub

[![.NET](https://img.shields.io/badge/.NET-ASP.NET%20Core%20MVC-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql)](https://www.postgresql.org/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-10B981)](#architecture)
[![Auth](https://img.shields.io/badge/Auth-JWT%20%2B%20RBAC-4F46E5)](#authentication--security)

**CourseHub** is a full-featured online course management and learning platform built with **C# / ASP.NET Core MVC** and **RESTful API** following **Clean Architecture**, **SOLID principles**, the **Repository Pattern**, and the **Result Pattern**.

Students can browse the course catalog, filter and search courses, manage their enrollments, and recover their passwords via email verification codes. Administrators have access to a dedicated management suite with real-time metrics, course/category CRUD, student account moderation, enrollment management, and an audit log of administrative actions.

---

## Table of Contents

- [Architecture & Design Patterns](#architecture--design-patterns)
- [Tech Stack](#tech-stack)
- [Key Features](#key-features)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#1-prerequisites)
  - [Configuration](#2-configuration)
  - [Database Setup & Migrations](#3-database-setup--migrations)
  - [Running the Application](#4-running-the-application)
- [Default Demo Accounts](#default-demo-accounts)
- [REST API Reference](#rest-api-reference)
- [Database Schema](#database-schema)

---

## Architecture & Design Patterns

The solution strictly adheres to **Clean Architecture** with clear separation of concerns across four projects:

```text
┌─────────────────────────────────────────────────────────────┐
│                           Web                               │
│   ASP.NET Core MVC Controllers, Razor Views, REST API,      │
│   Global Exception Middleware, JWT Authentication Setup     │
└───────────────┬─────────────────────────────┬───────────────┘
                │                             │
                ▼                             ▼
┌───────────────────────────────┐ ┌───────────────────────────┐
│          Application          │ │      Infrastructure       │
│  Business Services, DTOs,     │ │  EF Core DbContext,       │
│  Repository & Service         │◄┤  PostgreSQL Repositories, │
│  Interfaces, Result Pattern   │ │  JWT Generator, SMTP,     │
│                               │ │  Migrations & DbSeeder    │
└───────────────┬───────────────┘ └─────────────┬─────────────┘
                │                               │
                └───────────────┬───────────────┘
                                ▼
                ┌───────────────────────────────┐
                │            Domain             │
                │  Core Entities & Enumerations │
                │  (Zero external dependencies) │
                └───────────────────────────────┘
```

### Engineering Highlights
- **Result Pattern (`Result` & `Result<T>`)**: Standardized business operation outcomes without relying on exceptions for control flow.
- **Repository Pattern**: Full abstraction over `ApplicationDbContext` for testable, decoupled data access.
- **Class-based DTOs**: Explicit request/response Data Transfer Objects (`class` types with validation attributes) to prevent over-posting / mass assignment and avoid exposing EF Core entities directly.
- **Referential Integrity & Concurrency Safety**:
  - Composite unique database index on `Enrollments(UserId, CourseId)` to guarantee zero duplicate enrollments.
  - `DeleteBehavior.Restrict` on `Category -> Courses` to prevent orphaned courses when attempting to delete an active category.
- **Real-time Account Status Enforcement**: `JwtBearerEvents.OnTokenValidated` verifies the user's `IsActive` status on every request, immediately revoking access for blocked accounts even if their JWT has not yet expired.

---

## Tech Stack

| Layer | Technology |
| :--- | :--- |
| **Framework** | ASP.NET Core MVC & Web API (.NET) |
| **Database** | PostgreSQL |
| **ORM** | Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL`) |
| **Authentication** | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) + `HttpOnly` Cookie Support |
| **Password Security** | PBKDF2 Hashing (`Microsoft.AspNetCore.Identity.PasswordHasher<User>`) |
| **Email Service** | SMTP (`System.Net.Mail.SmtpClient`) + In-Memory Verification Code Cache (`IMemoryCache`) |
| **Frontend** | Razor Views, Bootstrap 5, Custom Responsive Design System (`site.css`) |

---

## Key Features

### Public & Student Features
- **Authentication & 3-Step Password Recovery**:
  - Student registration with server-side validation and unique email enforcement.
  - Sign-in & sign-out backed by JWT tokens.
  - **3-Step Forgot Password Flow via SMTP**:
    1. Enter registered email address to receive a 6-digit one-time verification code.
    2. Verify the 6-digit code in a dedicated confirmation screen.
    3. Set and confirm a new password upon successful code validation.
- **Interactive Course Catalog**:
  - Keyword search across course title, description, and instructor name.
  - Multi-parameter filtering by **category**, **price range (`MinPrice` / `MaxPrice`)**, and **availability status**.
  - Sorting by **newest**, **title (A–Z)**, **price: low to high**, and **price: high to low**.
  - Server-side pagination preserving active search and filter query parameters.
- **Course Enrollment Management**:
  - Detailed course view displaying curriculum description, instructor, duration, pricing, and remaining seat capacity.
  - One-click course enrollment and unenrollment with confirmation modals.
  - Dedicated **"My Courses"** dashboard and **"My Profile"** overview.

### Administrator Features (`Role = Admin`)
- **Admin Dashboard**:
  - Real-time KPI cards (total/available courses, categories, active/blocked students, total enrollments).
  - Latest student enrollments feed and recent administrative audit logs.
- **Course Management (CRUD)**:
  - Create, edit, preview, toggle availability, and delete courses.
  - Support for both **cover image file uploads** (`.jpg`, `.jpeg`, `.png`, `.webp`) and external image URLs.
  - Optional maximum student seat limit (`MaxStudents`) per course.
- **Category Management (CRUD)**:
  - Create, update, and delete categories with course counters.
  - Safe deletion guard preventing removal of categories linked to existing courses.
- **Student Management & Moderation**:
  - Paginated student directory with search by name/email and filtering by account status (`Active` / `Blocked`).
  - Detailed student profile view with full enrollment history.
  - Edit student details (`FirstName`, `LastName`, `Email`, `IsActive`) without exposing password hashes.
  - Instant account blocking and unblocking (restoration).
- **Course Roster & Enrollment Administration**:
  - View all students enrolled in a specific course.
  - Manually enroll a student into a course or remove an existing enrollment.
- **Administrative Audit Trail**:
  - Dedicated **Action Logs** page recording all create, update, delete, status-change, and enrollment operations performed by administrators.

---

## Project Structure

```text
CourseHub/
├── Domain/
│   ├── Entities/          # User, Course, Category, Enrollment, AdminActionLog
│   └── Enums/             # UserRole (Admin, Student)
├── Application/
│   ├── Common/            # Result, Result<T>, ErrorType, PagedResult<T>
│   ├── DTOs/              # Auth, Courses, Categories, Enrollments, Students, Admin DTOs
│   ├── Interfaces/        # IRepositories & IServices contracts
│   └── Services/          # Business logic implementations
├── Infrastructure/
│   ├── Authentication/    # JwtSettings, JwtTokenGenerator
│   ├── Data/              # ApplicationDbContext, DbSeeder
│   ├── Email/             # EmailSettings, SmtpEmailService
│   ├── Migrations/        # EF Core database migrations
│   └── Repositories/      # EF Core repository implementations
├── Web/
│   ├── Controllers/       # MVC Controllers & REST API Controllers (/api/*)
│   ├── Extensions/        # ClaimsPrincipal helper extensions
│   ├── Middleware/        # ExceptionHandlingMiddleware
│   ├── Models/            # ApiResponse, ViewModels
│   ├── Views/             # Razor Views (Courses, Account, Enrollments, Admin, Shared)
│   ├── wwwroot/           # Static assets (CSS, JS, uploaded course covers)
│   ├── appsettings.Example.json
│   └── Program.cs         # DI & HTTP pipeline configuration
└── database_schema.sql    # Standalone PostgreSQL schema script
```

---

## Getting Started

### 1. Prerequisites
- **.NET SDK** (10.0 or newer)
- **PostgreSQL** (14 or newer)

### 2. Configuration
Copy [`Web/appsettings.Example.json`](Web/appsettings.Example.json) to `Web/appsettings.json` (or configure via user secrets / environment variables) and update your PostgreSQL connection string, JWT signing key, and SMTP credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=course-hub_db;Username=postgres;Password=YOUR_DB_PASSWORD"
  },
  "JwtSettings": {
    "SecretKey": "REPLACE_WITH_YOUR_32_CHARS_MINIMUM_JWT_SECRET_KEY",
    "Issuer": "CourseHub",
    "Audience": "CourseHubUsers",
    "ExpiryMinutes": 1440
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "From": "your-email@gmail.com"
  },
  "SeedUsers": {
    "AdminEmail": "admin@coursehub.local",
    "AdminPassword": "CHANGE_ME_ADMIN_PASSWORD",
    "AdminFirstName": "AdminFirstName",
    "AdminLastName": "AdminLastName",
    "StudentEmail": "student@coursehub.local",
    "StudentPassword": "CHANGE_ME_STUDENT_PASSWORD",
    "StudentFirstName": "StudentFirstName",
    "StudentLastName": "StudentLastName"
  }
}
```

> **Note:** Never commit real production passwords, SMTP app passwords, or JWT signing keys to version control. Use `Web/appsettings.Example.json` as a template.

### 3. Database Setup & Migrations
On application startup, `DbSeeder.SeedAsync` automatically applies pending EF Core migrations (`Database.MigrateAsync()`) and seeds initial categories, sample courses, and default user accounts.

You can also apply migrations manually via the .NET CLI or execute [`database_schema.sql`](database_schema.sql):

```bash
dotnet ef database update --project Infrastructure --startup-project Web
```

### 4. Running the Application
From the root directory of the repository, run:

```bash
dotnet build
dotnet run --project Web
```

Then open your browser and navigate to the local URL shown in the terminal (e.g., `http://localhost:5000` or `https://localhost:5001`).

---

## Default Demo Accounts

The database seeder provisions two ready-to-use accounts configured via the `SeedUsers` section in `appsettings.json`:

| Role | Email | Password Source | Capabilities |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin@coursehub.local` | `SeedUsers:AdminPassword` | Full access to Admin Dashboard, Courses, Categories, Students, Enrollments, and Audit Logs |
| **Student** | `student@coursehub.local` | `SeedUsers:StudentPassword` | Browse catalog, enroll/unenroll in courses, view "My Courses" and "Profile" |

---

## REST API Reference

All REST API endpoints return a consistent JSON envelope (`ApiResponse` / `ApiResponse<T>`):

```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": { ... },
  "errors": null
}
```

| Method | Endpoint | Description | Access |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Register a new student account | Public |
| `POST` | `/api/auth/login` | Authenticate and obtain a JWT token | Public |
| `POST` | `/api/auth/forgot-password` | Send a 6-digit password reset code via email | Public |
| `POST` | `/api/auth/verify-reset-code` | Verify the 6-digit email reset code | Public |
| `POST` | `/api/auth/reset-password` | Set a new password using a verified code | Public |
| `POST` | `/api/auth/logout` | Clear authentication cookie | Authenticated |
| `GET` | `/api/auth/me` | Retrieve current authenticated user profile | Authenticated |
| `GET` | `/api/courses` | List courses with search, filtering, sorting & pagination | Public |
| `GET` | `/api/courses/{id}` | Get detailed course information by ID | Public |
| `POST` | `/api/courses` | Create a new course | `Admin` |
| `PUT` | `/api/courses/{id}` | Update an existing course | `Admin` |
| `DELETE` | `/api/courses/{id}` | Delete a course | `Admin` |
| `POST` | `/api/courses/{id}/enroll` | Enroll current student in a course | `Student` |
| `DELETE` | `/api/courses/{id}/enroll` | Cancel current student's course enrollment | `Student` |
| `GET` | `/api/me/courses` | List courses the current student is enrolled in | Authenticated |
| `GET` | `/api/categories` | List all categories with course counts | Public |
| `GET` | `/api/categories/{id}` | Get category details by ID | Public |
| `POST` | `/api/categories` | Create a new category | `Admin` |
| `PUT` | `/api/categories/{id}` | Update a category | `Admin` |
| `DELETE` | `/api/categories/{id}` | Delete an unused category | `Admin` |
| `GET` | `/api/admin/dashboard` | Retrieve platform metrics and recent activity | `Admin` |
| `GET` | `/api/admin/students` | List students with search, status filter & pagination | `Admin` |
| `GET` | `/api/admin/students/{id}` | Get student profile and enrollment history | `Admin` |
| `PUT` | `/api/admin/students/{id}` | Update student profile details | `Admin` |
| `PATCH` | `/api/admin/students/{id}/status` | Block or unblock (restore) a student account | `Admin` |
| `GET` | `/api/admin/courses/{id}/students` | List students enrolled in a specific course | `Admin` |
| `POST` | `/api/admin/courses/{id}/students/{studentId}` | Manually enroll a student in a course | `Admin` |
| `DELETE` | `/api/admin/courses/{id}/students/{studentId}` | Remove a student's enrollment from a course | `Admin` |

---

## Database Schema

The PostgreSQL database consists of 5 normalized tables managed by EF Core migrations (also available in [`database_schema.sql`](database_schema.sql)):

- **`Users`** (`Id`, `FirstName`, `LastName`, `Email` [UNIQUE], `PasswordHash`, `Role`, `IsActive`, `CreatedAt`)
- **`Categories`** (`Id`, `Name` [UNIQUE], `Description`)
- **`Courses`** (`Id`, `Title`, `InstructorName`, `Description`, `Duration`, `Price`, `IsAvailable`, `ImageUrl`, `MaxStudents`, `CreatedAt`, `CategoryId` [FK -> Categories ON DELETE RESTRICT])
- **`Enrollments`** (`Id`, `UserId` [FK -> Users ON DELETE CASCADE], `CourseId` [FK -> Courses ON DELETE CASCADE], `EnrollDate`, UNIQUE(`UserId`, `CourseId`))
- **`AdminActionLogs`** (`Id`, `AdminId`, `AdminEmail`, `Action`, `EntityName`, `EntityId`, `Details`, `CreatedAt`)
