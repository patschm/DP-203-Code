namespace SalesGenerator.EF;

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public double Price { get; set; }
    public Brand? Brand { get; set; }
}
