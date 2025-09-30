using Microsoft.AspNetCore.Mvc;
using TiendaUCN.API.Controllers;
using TiendaUCN.Application.DTOs.AuthResponse;
using TiendaUCN.Application.Services.Interfaces;

public class AuthController(IUserService userService) : BaseController
{
    private readonly IUserService _userservice = userService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var token = await _userservice.LoginAsync(loginDTO, HttpContext);
        return Ok(new { Token = token });
    }
}