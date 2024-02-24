namespace SalesGenerator.Cosmos;

public class Product : BaseDocument
{
    public Product() : base(nameof(Product))
    {
    }
    public string? BrandName { get; set; }
    public string? Name { get; set; }
    public double Price { get; set; }
}
