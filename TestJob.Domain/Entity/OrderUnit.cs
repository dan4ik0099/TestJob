namespace TestJob.Domain.Entity;

public class OrderUnit
{
    public Guid Id { get; set; }
    
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    
    public Guid ItemId { get; set; }
    public Item Item { get; set; }
    
    public int Count { get; set; }
    public decimal Cost { get; set; }
    public decimal TotalCost { get; set; }
}