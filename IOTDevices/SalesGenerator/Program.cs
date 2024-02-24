using CsvHelper;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Parquet.Serialization;
using SalesGenerator.Cosmos;
using SalesGenerator.EF;
using System.ComponentModel;
using System.Globalization;

namespace SalesGenerator;

internal class Program
{
    static int datasetSize = 100;
    static int pageNr = 1;
    static string mainFolder = "sales_small";
    static int orderId = 1;
    static int customerId = 1;
    static int productId = 1;
    static string sqlConnString = @"Server=.\SQLEXPRESS;Database=SalesDatabase;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";
    static string cosmoConnString = @"AccountEndpoint=https://4dn-cosmos-sales.documents.azure.com:443/;AccountKey=Lt0uq8Db4AHLSwxzRURygeDglXAyd75H2R05JMLdiTfCrb0NxE407MK27KbKFoV03nIY3xhcoO3CACDbUREiQg==;";

    static void Main(string[] args)
    {
        CreateFiles(mainFolder);        
        //CreateSqlData(sqlConnString);
        //CreateCosmosData(cosmoConnString);
    }

    private static void CreateFiles(string mainFolder)
    {
        if (Directory.Exists(mainFolder)) Directory.Delete(mainFolder, true);
        Directory.CreateDirectory(mainFolder);

        for (pageNr = 1; pageNr < 10; pageNr++)
        {
            var dataset = GenerateDataset(datasetSize);
            CreateCSV(dataset);
            CreateJson(dataset);
            CreateParquet(dataset);
        }
    }

    private static void CreateCosmosData(string cosmoConnString)
    {
        var context = new CosmosContext(cosmoConnString);
        if (!context.CreateDatabase())
        {
            return;
        }
        var dataset = GenerateDataset(datasetSize);

        var dsproduct = dataset.products.Select(p => new Cosmos.Product
        {
            Id = p.Id,
            BrandName = p.BrandName,
            Name = p.Name,
            Price = p.Price,
            PartitionKey = "product"
        }).ToList(); 
        context.AddProducts(dsproduct);

        var dscust = dataset.customers.Select(c =>
        {
            var doc = new Cosmos.Customer
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                LastName = c.LastName,
                FirstName = c.FirstName,
                Address = c.Address
            };
            doc.PartitionKey = doc.InternalId;
            return doc;
        }).ToList();
        context.AddCustomers(dscust);

        var dsorders = dataset.orders.Select(o => {
            var doc = new Cosmos.SalesOrder
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                ProductId = o.ProductId,
                Quantity = o.Quantity,
                TotalPrice = o.TotalPrice
            };
            doc.PartitionKey = doc.CustomerId.ToDocumentId(nameof(Cosmos.Customer));
            return doc;
        }).ToList();
        context.AddOrders(dsorders);
    }

    private static void CreateSqlData(string sqlConnString)
    {
        var bld = new DbContextOptionsBuilder();
        bld.UseSqlServer(sqlConnString);      
        var context = new EF.SqlDbContext(bld.Options);
        context.Database.EnsureCreated();

        var dataset = GenerateDataset(datasetSize);
        
        foreach(var product in dataset.products)
        {
            var prod = new EF.Product
            {
                Name = product.Name,
                Price = product.Price
            };
            var dbBrand = context.Brands.FirstOrDefault(b=>b.Name == product.BrandName);
            if (dbBrand == null)
                prod.Brand = new EF.Brand { Name = product.BrandName };
            else
                prod.Brand = dbBrand;

            context.Products.Add(prod);
            context.SaveChanges();
        }
        
        foreach(var customer in dataset.customers)
        {
            var address = new EF.Address
            {
                City = customer.Address?.City,
                Country = customer.Address?.Country,
                StreetName = customer.Address?.StreetName,
                Number = customer.Address!.Number,
                Region = customer.Address?.Region
            };
            context.Customers.Add(
                new EF.Customer { 
                    Address = address,
                    CompanyName = customer.CompanyName,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName
                });    
        }
        context.SaveChanges();
        foreach (var item in dataset.orders)
        {
            var order = new EF.SalesOrder
            {
                OrderDate = item.OrderDate,
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            };
            order.Customer = context.Customers.FirstOrDefault(c => c.Id == item.CustomerId);
            order.Product = context.Products.FirstOrDefault(p=>p.Id == item.ProductId);
            context.Orders.Add(order);
        }
        context.SaveChanges();
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
