using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.DTOs
{

    public record StudentResponseDto
    (
        int Id,
        string FullName,
        string Email
    );
    public record UpdateStudentDto
    (
        string FullName,
        string Email
    );
    public record CreateStudentDto
    (
        string FullName,
        string Email,
        string Password
    );
    public record TokenizedStudentDto(
        int Id,
        string FullName,
        string Email,
        string Token,
        DateTime ExpiresAt
    );
}
