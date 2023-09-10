using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace TestJob.Domain.Entity;

public class User : IdentityUser<Guid>
{
    [JsonIgnore]
    public List<CartUnit> Cart { get; set; } = new List<CartUnit>();
    [JsonIgnore]
    public List<Order> Orders { get; set; } = new List<Order>();
}