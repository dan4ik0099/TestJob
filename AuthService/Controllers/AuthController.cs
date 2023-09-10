using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.MiniService;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TestJob.Domain.Context;
using TestJob.Domain.Entity;
using TestJob.Domain.Request;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    private readonly ApplicationDbContext _applicationDbContext;
    private readonly RoleManager<Role> _roleManager;
    private readonly EmailSender _emailSender;
    private readonly SignInManager<User> _signInManager;


    public AuthController(UserManager<User> userManager, ApplicationDbContext applicationDbContext,
        RoleManager<Role> roleManager, EmailSender emailSender, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _signInManager = signInManager;
        if (_roleManager.RoleExistsAsync("User").Result) return;
        _roleManager.CreateAsync(new Role("User")).Wait();
        if (_roleManager.RoleExistsAsync("DraftUser").Result) return;
        _roleManager.CreateAsync(new Role("DraftUser")).Wait();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        var user = new User
        {
            UserName = registerRequest.Login,
            Email = registerRequest.Login
        };
        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (result.Succeeded)
        {
            var result1 = await _userManager.AddToRoleAsync(user, "DraftUser");
            if (result1.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink =
                    Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, code = token }, Request.Scheme);
                await _emailSender.SendEmailAsync(user.Email, "gg", confirmationLink);
                return Ok();
            }
        }

        return BadRequest();
    }

    private string GetToken(User user, IEnumerable<Claim> claimsCollection)
    {
        var claims = claimsCollection.ToList();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

        var signingCredentials =
            new SigningCredentials(
                new SymmetricSecurityKey("your_secret_key_that_is_long_enoughiahhazsxiogvhoiaigvhopiahgop"u8.ToArray()),
                SecurityAlgorithms.HmacSha256Signature);

        var jwt = new JwtSecurityToken(
            issuer: "https://localhost:7144/",
            audience: "https://localhost:7144/",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // 
            signingCredentials: signingCredentials);
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);


        return token;
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            await _userManager.RemoveFromRoleAsync(user, "DraftUser");
            await _userManager.AddToRoleAsync(user, "User");
            return Ok("Ваш адрес электронной почты успешно подтвержден.");
        }
        else
        {
            // Обработка ошибок подтверждения
            return BadRequest(result);
        }
        
    }


    [HttpPost("signIn")]
    public async Task<IActionResult> SignIn(SignInRequest signInRequest)
    {
        var user = await _userManager.FindByEmailAsync(signInRequest.Login);
        if (user == null)
        {
            return Unauthorized();
        }
        var result = await _signInManager.PasswordSignInAsync(user, signInRequest.Password, false, false);
        if (result.Succeeded)
        {
            IEnumerable<Claim> claims = await _userManager.GetClaimsAsync(user);
            return Ok(GetToken(user, claims));
        }

        return BadRequest();
    }

    [HttpPost("change-email")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> ChangeEmail(string newEmail)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.Email = newEmail;
            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            await _userManager.RemoveFromRoleAsync(user, "User");
            await _userManager.AddToRoleAsync(user, "DraftUser");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink =
                Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, code = token }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "gg", confirmationLink);
            return Ok();
        }

        return Unauthorized();
        
    }
    [HttpGet("info")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> InfoMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            
            return Ok(user);
        }

        return Unauthorized();
        
    }
}