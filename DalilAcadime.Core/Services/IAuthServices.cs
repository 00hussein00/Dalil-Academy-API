using System;
using System.Collections.Generic;
using System.Text;
using static DalilAcadime.Core.DTOs.LoginDTO;

namespace DalilAcadime.Core.Services
{
    public interface IAuthServices
    {
        Task<LoginResponseDto> Login(LoginRequestDto dto);
        Task<LoginResponseDto> Register(RegisterRequestDto dto);
    }
}
 