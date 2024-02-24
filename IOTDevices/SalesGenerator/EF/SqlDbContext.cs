using Microsoft.EntityFrameworkCore;

namespace SalesGenerator.EF;

public class SqlDbContext : DbContext
{
    public SqlDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> Orders => Set<SalesOrder>();
}
