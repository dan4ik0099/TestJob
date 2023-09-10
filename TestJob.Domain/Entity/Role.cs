using Microsoft.AspNetCore.Identity;

namespace TestJob.Domain.Entity;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
    }

    public Role(string roleName) : base(roleName)
    {
    }
}