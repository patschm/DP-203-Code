namespace SalesGenerator.EF;

public class Customer
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public Address? Address { get; set; }
    public ICollection<SalesOrder> Orders { get; set; } = new HashSet<SalesOrder>();    
}
