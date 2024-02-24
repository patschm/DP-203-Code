namespace SalesGenerator.EF;

public class Address
{
    public int Id { get; set; }
    public string? StreetName { get; set; }
    public int Number { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public ICollection<Customer> Customers { get; set; } = new HashSet<Customer>();
}