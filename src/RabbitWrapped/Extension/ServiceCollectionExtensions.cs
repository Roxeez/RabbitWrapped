using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitWrapped.Common;
using RabbitWrapped.Configuration;
using RabbitWrapped.Mapping;

namespace RabbitWrapped.Extension;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbit(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Rabbit");
        if (!section.Exists())
        {
            throw new InvalidOperationException("Can't found rabbit section in configuration");
        }

        return services.AddRabbit(new RabbitConfiguration
        {
            Host = section["Host"],
            Username = section["Username"],
            Password = section["Password"],
            Port = int.Parse(section["Port"])
        });
    }
    
    public static IServiceCollection AddRabbit(this IServiceCollection services, RabbitConfiguration configuration)
    {
        services.TryAddSingleton(configuration);
        services.TryAddSingleton<RabbitConnection>();
        services.TryAddSingleton<MappingManager>();
        services.AddHostedService<RabbitService>();
        return services;
    }

    public static IServiceCollection AddMessageConsumer<TConsumer>(this IServiceCollection services,
        string exchange = "", string routingKey = "", string queue = "") where TConsumer : MessageConsumer
    {
        services.TryAddTransient<TConsumer>();
        services.AddSingleton(new ConsumerMapping
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey,
            ConsumerType = typeof(TConsumer)
        });

        return services;
    }

    public static IServiceCollection AddMessageProducer<TMessage>(this IServiceCollection services,
        string exchange = "", string routingKey = "", string queue = "") where TMessage : IMessage
    {
        services.TryAddTransient<MessageProducer<TMessage>>();
        services.AddSingleton(new ProducerMapping
        {
            Queue = queue,
            Exchange = exchange,
            RoutingKey = routingKey,
            MessageType = typeof(TMessage)
        });

        return services;
    }
}