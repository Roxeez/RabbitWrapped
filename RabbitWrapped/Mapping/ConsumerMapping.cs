using System;

namespace RabbitWrapped.Common;

public class ConsumerMapping
{
    public string Queue { get; init; }
    public string Exchange { get; init; }
    public string RoutingKey { get; init; }
    
    public Type ConsumerType { get; init; }
}