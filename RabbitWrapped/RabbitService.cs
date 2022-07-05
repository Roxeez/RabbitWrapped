using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitWrapped.Common;

namespace RabbitWrapped;

internal class Consumer : IHostedService
{
    private readonly RabbitConnection connection;
    private readonly IEnumerable<ConsumerMapping> consumerMappings;
    private readonly IServiceScopeFactory scopeFactory;

    public Consumer(RabbitConnection connection, IEnumerable<ConsumerMapping> consumerMappings, IServiceScopeFactory scopeFactory)
    {
        this.connection = connection;
        this.consumerMappings = consumerMappings;
        this.scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        
        foreach (var mapping in consumerMappings)
        {
            var channel = connection.CreateChannel();
            var c = new EventingBasicConsumer(channel);

            var queue = channel.QueueDeclare(mapping.Queue, true, false, false);

            if (!string.IsNullOrEmpty(mapping.Exchange))
            {
                channel.ExchangeDeclare(mapping.Exchange, mapping.RoutingKey is null ? "fanout" : "direct", true, false);
                channel.QueueBind(queue, mapping.Exchange, mapping.RoutingKey);
            }
            
            c.Received += (sender, e) =>
            {
                var body = Encoding.UTF8.GetString(e.Body.Span);
                if (string.IsNullOrEmpty(body))
                {
                    return;
                }

                using var scope = scopeFactory.CreateScope();
                
                var consumer = scope.ServiceProvider.GetService(mapping.ConsumerType) as IMessageConsumer;
                if (consumer is null)
                {
                    return;
                }

                var message = JsonSerializer.Deserialize(body, consumer.MessageType) as IMessage;
                if (message is null)
                {
                    return;
                }
                
                consumer.Consume(message);
            };

            channel.BasicConsume(queue, true, c);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}