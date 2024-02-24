namespace SalesGenerator.EF;

public class SalesOrder
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public double TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer? Customer { get; set; }
    public Product? Product { get; set; }
}
