using System.ComponentModel.DataAnnotations;

namespace TestJob.Domain.Request;

public class SignInRequest
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Login { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}