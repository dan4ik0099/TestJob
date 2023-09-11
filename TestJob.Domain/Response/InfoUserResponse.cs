using Microsoft.AspNetCore.Identity;

namespace TestJob.Domain.Response;

public class InfoUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    
}