namespace TestJob.Domain.Entity;

public class Item
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string FullTitle { get; set; }
    public decimal Cost { get; set; }
}