namespace SalesGenerator.Cosmos;

public class Customer : BaseDocument
{
    public Customer() : base(nameof(Customer))
    {
        
    }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CompanyName { get; set; }
    public Address? Address { get; set; }
}
