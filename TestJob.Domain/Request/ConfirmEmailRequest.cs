namespace TestJob.Domain.Request;

public class ConfirmEmailRequest
{
    public string Token { get; set; }
    public string Email { get; set; }
}