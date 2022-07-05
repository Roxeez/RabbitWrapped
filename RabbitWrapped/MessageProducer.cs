using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitWrapped.Common;
using RabbitWrapped.Mapping;

namespace RabbitWrapped;

public class MessagePublisher<T> where T : IMessage
{
    private readonly RabbitConnection connection;
    private readonly MappingManager mappingManager;
    
    public MessagePublisher(RabbitConnection connection, MappingManager mappingManager)
    {
        this.connection = connection;
        this.mappingManager = mappingManager;
    }

    public void Publish(T message)
    {
        var mapping = mappingManager.GetPublisherMapping(message.GetType());
        if (mapping is null)
        {
            return;
        }
        
        var channel = connection.GetChannel();

        if (!string.IsNullOrEmpty(mapping.Queue))
        {
            channel.QueueDeclare(mapping.Queue, true, false, false);
        }

        if (!string.IsNullOrEmpty(mapping.Exchange))
        {
            channel.ExchangeDeclare(mapping.Exchange, mapping.RoutingKey is null ? "fanout" : "direct", true);
            if (!string.IsNullOrEmpty(mapping.Queue))
            {
                channel.QueueBind(mapping.Queue, mapping.Exchange, mapping.RoutingKey);
            }
        }

        var serialized = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(serialized);
        
        channel.BasicPublish(mapping.Exchange, mapping.RoutingKey ?? mapping.Queue, body: body);
    }
}