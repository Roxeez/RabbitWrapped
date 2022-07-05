using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using RabbitWrapped.Example.Message;

namespace RabbitWrapped.Example;

public class WorkerService : BackgroundService
{
    private readonly MessageProducer<MyMessage> producer;

    public WorkerService(MessageProducer<MyMessage> producer)
    {
        this.producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            producer.Produce(new MyMessage
            {
                Username = "test"
            });
        }
    }
}