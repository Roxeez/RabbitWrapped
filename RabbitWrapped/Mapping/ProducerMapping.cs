using System;

namespace RabbitWrapped.Mapping;

public class ProducerMapping
{
    public string Queue { get; init; }
    public string Exchange { get; init; }
    public string RoutingKey { get; init; }

    public Type MessageType { get; init; }
}