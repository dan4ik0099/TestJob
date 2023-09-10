using AutoMapper;

namespace TestJob.Domain.Response;

public class ItemCompactResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public decimal Cost { get; set; }
}