using DalilAcadime.Core.Exceptions;
using DalilAcadime.Core.Entities;
using DalilAcadime.Core.Repository;
using DalilAcadime.Core.Security;
using DalilAcadime.Core.Services;
using System;
using System.Collections.Generic;
using static DalilAcadime.Core.DTOs.LoginDTO;

namespace DalilAcadime.InfraStructures.Services
{
    public class AuthService (
        IUsersRepository usersRepository,
        IStudentRepository studentRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService
        ) : IAuthServices
    {
        public async Task<LoginResponseDto> Login(LoginRequestDto dto)
        {
            var user = await usersRepository.GetUserByEmail(dto.Email);
            if (user == null || !passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Invalid email or password.");
            }
            
            var (token , ExpiresAt) = tokenService.GenerateToken(user);

            return new LoginResponseDto (token,ExpiresAt );
        }

        public async Task<LoginResponseDto> Register(RegisterRequestDto dto)
        {
            var student = new Student
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHasher.HashPassword(dto.Password),
                Role = UserRoles.Student
            };

            var createdStudent = await studentRepository.CreateStudent(student);
            return new LoginResponseDto(createdStudent.Token, createdStudent.ExpiresAt);
        }
    }
}
