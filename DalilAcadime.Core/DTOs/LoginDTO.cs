using System;
using System.Collections.Generic;
using System.Text;

namespace DalilAcadime.Core.DTOs
{
    public class LoginDTO 
    {
        public record LoginRequestDto(string Email, string Password);
        public record RegisterRequestDto(string FullName, string Email, string Password);
        public record LoginResponseDto(string Token, DateTime ExpiresAt);

    }
}
