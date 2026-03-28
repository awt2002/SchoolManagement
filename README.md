# School Management System

A RESTful School Management API built with ASP.NET Core 9 and a Blazor WebAssembly frontend. It provides JWT-based authentication with refresh tokens, role-based authorization, password reset by email, and complete school modules for students, teachers, classes, attendance, grades, exams, announcements, and dashboard insights.

## Features

- JWT Authentication – Access token + refresh token flow with configurable expiry
- Role-Based Authorization – `Admin`, `Teacher`, and `Student` roles with protected endpoints
- Password Reset – Forgot-password and reset-password API flow with email support
- School Modules – Students, Teachers, Classes, Academic Years, Subjects, Attendance, Grades, Exams, Announcements, Dashboard
- Swagger / OpenAPI – Interactive API documentation with Bearer token support
- Auto-Seeding – Academic year, demo users, classes, students, teachers, subjects, grades, attendance, exams, and announcements are created on first run

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9 (.NET 9) + Blazor WebAssembly |
| Authentication | Custom JWT + refresh token cookie flow |
| Database | SQL Server + Entity Framework Core |
| UI | Blazor WebAssembly + MudBlazor |
| Email | SMTP |
| API Docs | Swagger / OpenAPI (Swashbuckle) |

## Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB, Express, or full instance)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/awt2002/SchoolManagement.git
cd SchoolManagement
```

### 2. Configure `appsettings.json`

Open `backend/SMS.API/appsettings.json` and update the sections below.

#### Connection String

Point to your SQL Server instance:

```json
"ConnectionStrings": {
  "Default": "Server=YOUR_SERVER;Database=SchoolManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```



#### Email (SMTP)

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "User": "youremail@gmail.com",
  "Password": "your-app-password", // https://myaccount.google.com/apppasswords
  "FromName": "SchoolManagement"
}
```

### 3. Apply database migrations

```bash
cd backend/SMS.API
dotnet ef database update
```

### 4. Run the application

```bash
cd backend/SMS.API
dotnet run
```

The API starts at `https://localhost:7056` by default.


### 5. Explore with Swagger

Open `https://localhost:7056/swagger` to view and test endpoints.



## API Endpoints

### Authentication — `/api/v1/auth/...`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/login` | Login and receive JWT + refresh token cookie |
| POST | `/refresh` | Exchange refresh token cookie for a new JWT |
| POST | `/logout` | Revoke refresh token and sign out |
| POST | `/forgot-password` | Send password reset email |
| POST | `/reset-password` | Submit new password with reset token |
| POST | `/change-password` | Change current user password (authorized) |

### Core Modules — `/api/v1/...`

| Controller | Base Route |
|---|---|
| Students | `/api/v1/students` |
| Teachers | `/api/v1/teachers` |
| Classes | `/api/v1/classes` |
| Academic Years | `/api/v1/academic-years` |
| Attendance | `/api/v1/attendance` |
| Subjects | `/api/v1/subjects` |
| Grades | `/api/v1/grades` |
| Exams | `/api/v1/exams` |
| Announcements | `/api/v1/announcements` |
| Dashboard | `/api/v1/dashboard` |

## API Response Format

All endpoints use a consistent response envelope style:

```json
{
  "success": true,
  "message": "Descriptive message",
  "data": {},
  "errors": [],
  "statusCode": 200
}
```

## Request Body Examples

### Login

`POST /api/v1/auth/login`

```json
{
  "username": "admin",
  "password": "Admin123"
}
```

### Forgot Password

`POST /api/v1/auth/forgot-password`

```json
{
  "email": "youremail@gmail.com"
}
```

### Change Password

`POST /api/v1/auth/change-password`
`Authorization: Bearer <token>`

```json
{
  "currentPassword": "Strong@123",
  "newPassword": "NewStrong@456",
  "confirmNewPassword": "NewStrong@456"
}
```

## Default Seeded Accounts

Created automatically on first run:

- Admin: `admin` / `Admin123`
- Teachers: sample teacher users with password `Teacher123`
- Students: sample student users with password `Student123`

## Project Structure

```text
SchoolManagement/
├── backend/
│   ├── SMS.API/
│   │   ├── Controllers/
│   │   ├── Extensions/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── SMS.API.csproj
│   ├── SMS.Application/
│   │   ├── Features/
│   │   ├── Interfaces/
│   │   ├── Common/
│   │   └── SMS.Application.csproj
│   ├── SMS.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   └── SMS.Domain.csproj
│   └── SMS.Infrastructure/
│       ├── Data/
│       ├── Services/
│       ├── Seed/
│       └── SMS.Infrastructure.csproj
└── frontend/
    └── SMS.Blazor/
        ├── Pages/
        ├── Services/
        ├── Auth/
        ├── Program.cs
        └── SMS.Blazor.csproj
```

## License

This project is open source.
