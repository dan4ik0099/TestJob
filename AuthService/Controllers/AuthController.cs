using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.MiniService;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TestJob.Domain.Context;
using TestJob.Domain.Entity;
using TestJob.Domain.Request;
using TestJob.Domain.Response;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IMapper _mapper;
    private readonly RoleManager<Role> _roleManager;
    private readonly EmailSender _emailSender;
    private readonly SignInManager<User> _signInManager;


    public AuthController(UserManager<User> userManager, ApplicationDbContext applicationDbContext,
        RoleManager<Role> roleManager, EmailSender emailSender, SignInManager<User> signInManager, IMapper mapper)
    {
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _signInManager = signInManager;
        _mapper = mapper;
    }

    [HttpPost]
    [Route(("register"))]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var user = new User
        {
            UserName = registerRequest.Login,
            Email = registerRequest.Login
        };
        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded)
        {
            return BadRequest();
        }

        var result1 = await _userManager.AddToRoleAsync(user, "DraftUser");
        if (!result1.Succeeded)
        {
            return BadRequest();
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink =
            Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, code = token }, Request.Scheme);
        await _emailSender.SendEmailAsync(user.Email, "gg", confirmationLink);
        return Ok();
    }

    private static string GetToken(User user, IEnumerable<Claim> claimsCollection)
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

    [HttpGet]
    [Route("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        await _userManager.RemoveFromRoleAsync(user, "DraftUser");
        await _userManager.AddToRoleAsync(user, "User");
        return Ok("Ваш адрес электронной почты успешно подтвержден.");

    }


    [HttpPost]
    [Route(("sign-in"))]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest signInRequest)
    {
        var user = await _userManager.FindByEmailAsync(signInRequest.Login);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await _signInManager.PasswordSignInAsync(user, signInRequest.Password, false, false);
        if (!result.Succeeded)
        {
            return BadRequest();
        }
        IEnumerable<Claim> claims = await _userManager.GetClaimsAsync(user);
        return Ok(GetToken(user, claims));

    }

    [HttpPost]
    [Route("change-email")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> ChangeEmail([FromBody] string newEmail)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        if (user is null)
        {
            return Unauthorized();
        }

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

    [HttpGet]
    [Route(("info"))]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> InfoMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }
        var response = _mapper.Map<InfoUserResponse>(user);
        return Ok(response);
    }
}