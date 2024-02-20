namespace SalesGenerator;

public class SalesOrder
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
}
