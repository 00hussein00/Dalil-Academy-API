# Dalil Academy API

Backend API for a Learning Management System built with **ASP.NET Core 10**. The project provides APIs for authentication, users, students, courses, enrollments, and instructors.

## Technologies

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL
- JWT Authentication
- BCrypt password hashing
- Swagger / OpenAPI
- FluentValidation

## Requirements

Install the following before running the project:

- .NET 10 SDK
- PostgreSQL
- Git

## Clone the Project

```bash
git clone https://github.com/00hussein00/Dalil-Academy-API.git
cd Dalil-Academy-API
```

## Database Setup

Create a PostgreSQL database named `DalilAcademyDB`.

The default connection uses:

```text
Host=localhost;Port=5432;Database=DalilAcademyDB;Username=postgres
```

Do not store your database password in Git. Use User Secrets for local development:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=DalilAcademyDB;Username=postgres;Password=YOUR_POSTGRES_PASSWORD" --project .\DalilAcadime.API\DalilAcadime.API.csproj
dotnet user-secrets set "Jwt:Key" "YOUR_LONG_RANDOM_JWT_KEY" --project .\DalilAcadime.API\DalilAcadime.API.csproj
```

Use a long, random JWT key and never commit it to GitHub.

## Apply Migrations

Run this command from the solution directory:

```bash
dotnet ef database update --project .\DalilAcadime.InfraStructures\DalilAcadime.InfraStructures.csproj --startup-project .\DalilAcadime.API\DalilAcadime.API.csproj
```

If Entity Framework CLI is not installed, run:

```bash
dotnet tool install --global dotnet-ef
```

The first database initialization seeds sample users, instructors, and courses.

## Run the Project

```bash
dotnet restore
dotnet run --project .\DalilAcadime.API\DalilAcadime.API.csproj
```

In Development mode, open the Swagger URL displayed in the terminal. Swagger is configured at the application root, usually:

```text
https://localhost:xxxx/
```

## Authentication

Use the following endpoint to log in:

```text
POST /api/Auth/login
```

After receiving a JWT, click **Authorize** in Swagger and enter:

```text
Bearer YOUR_TOKEN
```

Sample development users:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@a.com` | `admin123` |
| Instructor | `instructor@i.com` | `instructor123` |
| Student | `student@s.com` | `student123` |

These credentials are for local development only. Change or remove them before production deployment.

## Main Endpoints

- `POST /api/Auth/login` - Log in and receive a JWT.
- `POST /api/Auth/register` - Register a new user.
- `GET /api/Courses` - Get paginated courses with price filtering.
- `GET /api/Courses/{id}` - Get a course by ID.
- `GET /api/Courses/Count` - Get the total number of courses.
- Student, enrollment, and user endpoints are available through Swagger according to the required permissions.

## Project Structure

- `DalilAcadime.API` - Controllers, application configuration, and Swagger.
- `DalilAcadime.Core` - DTOs, entities, interfaces, and validators.
- `DalilAcadime.InfraStructures` - Database context, repositories, services, security, and migrations.

## Security Notes

- Never commit passwords, JWT keys, or other secrets to GitHub.
- Use User Secrets locally and environment variables or a secret manager in production.
- Change the sample user credentials before deploying the application.
