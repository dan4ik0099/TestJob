namespace TestJob.Domain.Entity;

public class CartUnit
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    public Guid ItemId { get; set; }
    public Item Item { get; set; }

    public decimal Cost { get; set; }
    public int Count { get; set; }

    public decimal TotalCost { get; set; }
}