using DalilAcadime.Core.Repository;
using DalilAcadime.Core.Security;
using DalilAcadime.Core.Services;
using DalilAcadime.Core.Storage;
using DalilAcadime.Core.Validetors;
using DalilAcadime.InfraStructures.Data;
using DalilAcadime.InfraStructures.Repository;
using DalilAcadime.InfraStructures.Security;
using DalilAcadime.InfraStructures.Services;
using DalilAcadime.InfraStructures.Storage;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Dalil Academy LMS API",
        Version = "v1",
        Description = "Learning Management System API built with ASP.NET Core 10"
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {

        Type = SecuritySchemeType.Http,

        Scheme = "bearer",

        BearerFormat = "JWT",

        Description = "Paste the token from /api/Auth/login"

    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement

    {

        [new OpenApiSecuritySchemeReference("Bearer", document)] = []

    });

});

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseServices,CourseServices>();
//---
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthServices, AuthService>();
builder.Services.AddSingleton<IImageService, ImgService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateCourseDtoValidators>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateEnrollmentDtoValidator>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(opt => 
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSetting>();
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.FromMinutes(5)
    }
);

builder.Services.AddAuthorization();

// for configration and service
// ------------------------------------------------------------------------------
var app = builder.Build();
// ------------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dalil Academy API v1");
        options.RoutePrefix = string.Empty; // Swagger at root
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

