using DalilAcadime.Core.DTOs;
using DalilAcadime.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.Mappings
{
    public static class StudentMapping
    {
        public static StudentResponseDto ToStudentResponseDto(this Student student)
        {
            return new StudentResponseDto(
                student.Id,
                student.FullName,
                student.Email
            );
        }
        public static Student ToStudentEntity(this CreateStudentDto dto)
        {
            return new Student
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = dto.Password
            };
        }
    }
}
