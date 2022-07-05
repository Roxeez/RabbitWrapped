using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitWrapped.Common;
using RabbitWrapped.Mapping;

namespace RabbitWrapped;

internal class RabbitService : IHostedService
{
    private readonly RabbitConnection connection;
    private readonly ILogger<RabbitService> logger;
    private readonly MappingManager mappingManager;
    private readonly IServiceScopeFactory scopeFactory;

    public RabbitService(RabbitConnection connection, IServiceScopeFactory scopeFactory, MappingManager mappingManager,
        ILogger<RabbitService> logger)
    {
        this.connection = connection;
        this.scopeFactory = scopeFactory;
        this.mappingManager = mappingManager;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetEntryAssembly()!;

        var mappings = mappingManager.GetConsumerMappings();
        foreach (var mapping in mappings)
        {
            var channel = connection.CreateChannel();
            var c = new EventingBasicConsumer(channel);

            logger.LogDebug("Declaring queue {queue}", mapping.Queue);
            var queue = channel.QueueDeclare(mapping.Queue, true, false, false);

            if (!string.IsNullOrEmpty(mapping.Exchange))
            {
                logger.LogDebug("Declaring exchange {exchange}", mapping.Exchange);
                channel.ExchangeDeclare(mapping.Exchange, mapping.RoutingKey is null ? "fanout" : "direct", true);

                logger.LogDebug("Binding queue {queue} to exchange {exchange} (Routing key: {routingKey})",
                    mapping.Queue, mapping.Exchange, mapping.RoutingKey ?? "NONE");
                channel.QueueBind(queue, mapping.Exchange, mapping.RoutingKey);
            }

            c.Received += (sender, e) =>
            {
                var body = Encoding.UTF8.GetString(e.Body.Span);
                if (string.IsNullOrEmpty(body))
                {
                    logger.LogError("Message body is empty ({consumerType})", mapping.ConsumerType.Name);
                    return;
                }

                try
                {
                    using var scope = scopeFactory.CreateScope();

                    var consumer = scope.ServiceProvider.GetService(mapping.ConsumerType) as MessageConsumer;
                    if (consumer is null)
                    {
                        logger.LogError("Failed to create instance of {consumer}", mapping.ConsumerType.Name);
                        return;
                    }

                    var deserialized = JsonSerializer.Deserialize(body, consumer.MessageType) as IMessage;
                    if (deserialized is null)
                    {
                        logger.LogError("Failed to deserialize body into {messageType}", consumer.MessageType.Name);
                        return;
                    }

                    logger.LogDebug("Consuming message {messageType} using consumer {consumerType}",
                        consumer.MessageType.Name, mapping.ConsumerType.Name);
                    consumer.Consume(deserialized);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to consume message");
                }
            };

            channel.BasicConsume(queue, true, $"{connection.Name}", c);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}