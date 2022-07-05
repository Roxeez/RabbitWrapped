using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitWrapped.Common;
using RabbitWrapped.Mapping;

namespace RabbitWrapped;

public class MessageProducer<T> where T : IMessage
{
    private readonly RabbitConnection connection;
    private readonly ILogger<MessageProducer<T>> logger;
    private readonly MappingManager mappingManager;

    public MessageProducer(RabbitConnection connection, MappingManager mappingManager,
        ILogger<MessageProducer<T>> logger)
    {
        this.connection = connection;
        this.mappingManager = mappingManager;
        this.logger = logger;
    }

    public void Produce(T message)
    {
        var mapping = mappingManager.GetProducerMapping(message.GetType());
        if (mapping is null)
        {
            logger.LogError("Can't found mapping for message {message}", message.GetType());
            return;
        }

        var channel = connection.GetChannel();

        if (!string.IsNullOrEmpty(mapping.Queue))
        {
            logger.LogDebug("Declaring queue {queue}", mapping.Queue);
            channel.QueueDeclare(mapping.Queue, true, false, false);
        }

        if (!string.IsNullOrEmpty(mapping.Exchange))
        {
            logger.LogDebug("Declaring exchange {exchange}", mapping.Exchange);
            channel.ExchangeDeclare(mapping.Exchange, mapping.RoutingKey is null ? "fanout" : "direct", true);
            if (!string.IsNullOrEmpty(mapping.Queue))
            {
                logger.LogDebug("Binding queue {queue} to exchange {exchange} (Routing key: {routingKey})",
                    mapping.Queue, mapping.Exchange, mapping.RoutingKey ?? "NONE");
                channel.QueueBind(mapping.Queue, mapping.Exchange, mapping.RoutingKey);
            }
        }

        var serialized = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(serialized);

        logger.LogDebug("Publishing message {message}", message.GetType());
        channel.BasicPublish(mapping.Exchange,
            string.IsNullOrEmpty(mapping.RoutingKey) ? mapping.Queue : mapping.RoutingKey, body: body);
    }
}