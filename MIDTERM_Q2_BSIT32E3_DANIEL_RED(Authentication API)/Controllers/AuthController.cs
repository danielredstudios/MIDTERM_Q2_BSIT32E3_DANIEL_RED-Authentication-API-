using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MIDTERM_Q2_BSIT32E3_DANIEL_RED__Authentication_API_.Models;

namespace MIDTERM_Q2_BSIT32E3_DANIEL_RED__Authentication_API_.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration) : Controller
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest request)
    {
        var user = new IdentityUser { UserName = request.UserName, Email = request.UserName };
        var result = await userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
            return Ok(new { Message = "User registered successfully" });

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid credentials");

        var token = CreateToken(user);
        return Ok(new { Token = token });
    }

    private string CreateToken(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

