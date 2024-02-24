namespace SalesGenerator.Cosmos;

public class SalesOrder : BaseDocument
{
    public SalesOrder() : base(nameof(SalesOrder))
    {
        
    }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public long CustomerId { get; set; }
    public long ProductId { get; set; }
}
