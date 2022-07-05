using System;
using System.Collections.Generic;
using System.Linq;

namespace RabbitWrapped.Mapping;

public class MappingManager
{
    private readonly IEnumerable<ConsumerMapping> consumerMappings;
    private readonly Dictionary<Type, ProducerMapping> producerMappings;

    public MappingManager(IEnumerable<ConsumerMapping> consumerMappings, IEnumerable<ProducerMapping> producerMappings)
    {
        this.consumerMappings = consumerMappings;
        this.producerMappings = producerMappings.ToDictionary(x => x.MessageType);
    }

    public IEnumerable<ConsumerMapping> GetConsumerMappings()
    {
        return consumerMappings;
    }

    public ProducerMapping GetProducerMapping(Type messageType)
    {
        return producerMappings.GetValueOrDefault(messageType);
    }
}