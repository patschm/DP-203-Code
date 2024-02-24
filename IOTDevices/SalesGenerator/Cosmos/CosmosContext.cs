using Microsoft.Azure.Cosmos;

namespace SalesGenerator.Cosmos;

public class CosmosContext
{
    private readonly CosmosClient _cosmosClient;
    public string DatabaseName { get; set; } = "SalesDB";
    public string ContainerName { get; set; } = "sales";

    public CosmosContext(string conString)
    {
        _cosmosClient = new CosmosClient(conString);
    }
    internal bool CreateDatabase()
    {
        var dbResp = _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName).Result;
        if (dbResp.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Task.Delay(2000).Wait();
            var cResp = dbResp.Database
                .CreateContainerIfNotExistsAsync(ContainerName, "/partitionkey")
                .Result;
            if (cResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Container created");
                return true;
            }
            else
            {
                Console.WriteLine($"Container Creation Failed ({cResp.StatusCode})");
            }
            Task.Delay(1000).Wait();
        }
        else
        {
            Console.WriteLine($"Creation Database Failed ({dbResp.StatusCode})");
        }
        return false;
    }
    public void AddProducts(List<Product> products)
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        foreach (var doc in products)
        {
            try
            {
               container
                    .CreateItemAsync(doc, partitionKey: new PartitionKey(doc.PartitionKey))
                    .Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    public void AddCustomers(List<Customer> customers) 
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        foreach (var doc in customers)
        {
            try
            {
                container
                    .CreateItemAsync(doc, partitionKey: new PartitionKey(doc.PartitionKey))
                    .Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    public void AddOrders(List<SalesOrder> orders) 
    {
        var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);
        foreach (var doc in orders)
        {
            try
            {
                container
                    .CreateItemAsync(doc, partitionKey: new PartitionKey(doc.PartitionKey))
                    .Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
