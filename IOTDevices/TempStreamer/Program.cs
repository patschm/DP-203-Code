
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;

namespace TempStreamer;

internal class Program
{
    const string conStr = "Endpoint=sb://hubfordotnet.servicebus.windows.net/;SharedAccessKeyName=Writer;SharedAccessKey=v8KB7EZbsU20G0d+lTrNEIjRTnhEVFKyv+AEhAAZbDg=;EntityPath=events";
    const string hubName = "events";
    private static Random rnd = new Random();

    static void Main(string[] args)
    {
        var tasks = new List<Task>();
        for (var i = 1; i < 2; i++)
        {
            tasks.Add(SendEventAsync(i));
        }
        Task.WhenAll(tasks).Wait();
        Console.WriteLine("Done!");
        Console.ReadLine();
    }

    private static async Task SendEventAsync(int device)
    {
        Console.WriteLine($"{device} starts sending...");
        await using (var producerClient = new EventHubProducerClient(conStr, hubName))
        {
            int series = 0;
            do
            {
                series++;
                var eventBatch = await producerClient.CreateBatchAsync();
                for (int j = 0; j < 5; j++)
                {
                    eventBatch.TryAdd(new EventData
                    {
                        ContentType = "text/plain",
                        MessageId = $"{series}+{j}",
                        CorrelationId = $"{device}_{series}",
                        EventBody = new BinaryData(new
                        {
                            CreationTime = DateTime.Now,
                            Device = device,
                            Series = series,
                            Value = rnd.Next(0, 150),
                            MinTemp = 0,
                            MaxTemp = 150
                        })
                    });
                }
                await producerClient.SendAsync(eventBatch);
                await Task.Delay(1000);
            }
            while (series < 10);
        }
    }
}
