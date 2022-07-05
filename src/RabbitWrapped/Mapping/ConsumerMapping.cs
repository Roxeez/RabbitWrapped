using System;

namespace RabbitWrapped.Mapping;

public class ConsumerMapping
{
    public string Queue { get; init; }
    public string Exchange { get; init; }
    public string RoutingKey { get; init; }

    public Type ConsumerType { get; init; }
}