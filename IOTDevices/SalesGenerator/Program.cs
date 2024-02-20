using CsvHelper;
using Newtonsoft.Json;
using Parquet.Serialization;
using System.Globalization;

namespace SalesGenerator;

internal class Program
{
    static int pageNr = 1;
    static string mainFolder = "sales_small";
    static int orderId = 1;
    static int customerId = 1;
    static int productId = 1;

    static void Main(string[] args)
    {
        if (Directory.Exists(mainFolder)) Directory.Delete(mainFolder, true);
        Directory.CreateDirectory(mainFolder);

        var nrOfSales = 100;
        for (pageNr = 1; pageNr < 10; pageNr++)
        {
            var dataset = GenerateDataset(nrOfSales);
            CreateCSV(dataset);
            CreateJson(dataset);
            CreateParquet(dataset);
        }
    }

    private static void CreateParquet((List<Customer> customers, List<Product> products, List<SalesOrder> orders) dataset)
    {
        var dir = Directory.CreateDirectory($@"{mainFolder}\parquet");
        using var writer = new FileStream($@"{dir.FullName}\customers_{pageNr}.parquet", FileMode.OpenOrCreate);
        ParquetSerializer.SerializeAsync(dataset.customers, writer);
        using var writer2 = new FileStream($@"{dir.FullName}\products_{pageNr}.parquet", FileMode.OpenOrCreate);
        ParquetSerializer.SerializeAsync(dataset.products, writer2);
        using var writer3 = new FileStream($@"{dir.FullName}\orders_{pageNr}.parquet", FileMode.OpenOrCreate);
        ParquetSerializer.SerializeAsync(dataset.orders, writer3);
    }

    private static void CreateJson((List<Customer> customers, List<Product> products, List<SalesOrder> orders) dataset)
    {
        var customers = "customers";
        var products = "products";
        var orders = "orders";

        var serializer = new JsonSerializer();
        var dir = Directory.CreateDirectory($@"{mainFolder}\json\{customers}_{pageNr}");
        foreach (var customer in dataset.customers)
        {
            using var writer = new StreamWriter($@"{dir.FullName}\c{customer.Id}.json");
              serializer.Serialize(writer, customer);
        }
        dir = Directory.CreateDirectory($@"{mainFolder}\json\{products}_{pageNr}");
        foreach(var product in dataset.products)
        {
            using var writer2 = new StreamWriter($@"{dir.FullName}\p{product.Id}.json");
                serializer.Serialize(writer2, dataset.products);
        }
        
        dir = Directory.CreateDirectory($@"{mainFolder}\json\{orders}_{pageNr}");
        foreach (var order in dataset.orders)
        {
            using var writer3 = new StreamWriter($@"{dir.FullName}\o{order.Id}.json");
              serializer.Serialize(writer3, dataset.orders);
        }
    }

    private static void CreateCSV((List<Customer> customers, List<Product> products, List<SalesOrder> orders) dataset)
    {
        var dir = Directory.CreateDirectory($@"{mainFolder}\csv");
        using var writer = new StreamWriter($@"{dir.FullName}\customers_{pageNr}.csv");
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(dataset.customers);
        }
        using var writer2 = new StreamWriter($@"{dir.FullName}\products_{pageNr}.csv");
        using (var csv = new CsvWriter(writer2, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(dataset.products);
        }
        using var writer3 = new StreamWriter($@"{dir.FullName}\orders_{pageNr}.csv");
        using (var csv = new CsvWriter(writer3, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(dataset.orders);
        }
    }

    private static (List<Customer> customers, List<Product> products, List<SalesOrder> orders) GenerateDataset(int nrOfSales)
    {
        var customers = GenerateCustomers(nrOfSales/10);
        var products = GenerateProducts(nrOfSales / 5);
        var orders = GenerateOrders(nrOfSales, customers, products);

        return (customers, products, orders);
    }

    private static List<SalesOrder> GenerateOrders(int nrOfSales, List<Customer> customers, List<Product> products)
    {
        return new Bogus.Faker<SalesOrder>("nl")
            .RuleFor(c => c.Id, f => orderId++)
            .RuleFor(c=>c.Quantity, f=>f.Random.Int(1, 4))
            .RuleFor(c=>c.ProductId, f => f.PickRandom(products).Id)
            .RuleFor(c=>c.CustomerId, f=>f.PickRandom(customers).Id)
            .RuleFor(c=>c.OrderDate, f=>f.Date.Between(DateTime.Now.AddYears(-5), DateTime.Now))    
            .RuleFor(c=>c.TotalPrice, (f, so)=>products.Where(p=>so.ProductId == p.Id).Select(p=>p.Price * so.Quantity).First())
            .Generate(nrOfSales)
            .ToList();        
    }

    private static List<Product>GenerateProducts(int nrProducts)
    {
        string[] brands = { "Sony", "Nikon", "Philips", "Samsung", "LG", "Olympus", "Canon", "Braun" };
        return new Bogus.Faker<Product>("nl")
            .RuleFor(c => c.Id, f => productId++)
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.BrandName, f => f.PickRandom(brands))
            .RuleFor(p => p.Price, f => f.Commerce.Random.Double(10, 1000))
            .Generate(nrProducts)
            .ToList();
    }

    private static List<Customer> GenerateCustomers(int nrOfCustomers)
    {
        (string, string)[] cities = { 
            ("Amsterdam", "Noord Holland"), 
            ("Rotterdam", "Zuid Holland"), 
            ("Den Haag", "Zuid Holland"), 
            ("Utrecht", "Utrecht"), 
            ("Eindhoven", "Noord Brabant"), 
            ("Almere", "Flevoland"), 
            ("Groningen", "Groningen"), 
            ("Tilburg", "Noord Brabant"), 
            ("Breda", "Noord Brabant"), 
            ("Nijmegen", "Gelderland") };
        return new Bogus.Faker<Customer>("nl")
            .RuleFor(c => c.Id, f => customerId++)
            .RuleFor(c => c.FirstName, f => f.Person.FirstName)
            .RuleFor(c => c.LastName, f => f.Person.LastName)
            .RuleFor(c => c.Address, f => {
                var city = f.PickRandom(cities);
                return new Address
                {
                    City = city.Item1,
                    Region = city.Item2,
                    Country = "Nederland",
                    StreetName = f.Address.StreetName(),
                    Number = f.Address.Random.Number(1, 100)
                };
            })
            .RuleFor(c => c.CompanyName, f => f.Company.CompanyName(0))
            .Generate(nrOfCustomers)
            .ToList();
    }

}
