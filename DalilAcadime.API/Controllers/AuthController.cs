using DalilAcadime.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DalilAcadime.Core.DTOs.LoginDTO;

namespace DalilAcadime.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthServices authServices) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
        {
            return Ok(await authServices.Login(dto));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Register(RegisterRequestDto dto)
        {
            return Ok(await authServices.Register(dto));
        }
    }
}
