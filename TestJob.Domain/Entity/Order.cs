using System.Text.Json.Serialization;

namespace TestJob.Domain.Entity;

public class Order
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; }

    [JsonIgnore]
    public List<OrderUnit> OrderUnits { get; set; } = new List<OrderUnit>();
    
    public DateTime Date { get; set; }
    public decimal TotalCost { get; set; }
    
}