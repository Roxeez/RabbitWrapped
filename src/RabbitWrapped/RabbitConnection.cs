using System;
using System.Reflection;
using RabbitMQ.Client;
using RabbitWrapped.Configuration;

namespace RabbitWrapped.Common;

public class RabbitConnection : IDisposable
{
    private readonly ConnectionFactory factory;
    private IModel channel;
    private int channelCount;

    private IConnection connection;

    public string Name => connection?.ClientProvidedName;

    public RabbitConnection(RabbitConfiguration configuration)
    {
        var assembly = Assembly.GetEntryAssembly()!;
        factory = new ConnectionFactory
        {
            HostName = configuration.Host,
            UserName = configuration.Username,
            Password = configuration.Password,
            Port = configuration.Port,
            ClientProvidedName = $"{assembly.GetName().Name} ({assembly.GetName().Version})"
        };
    }

    public void Dispose()
    {
        channel.Dispose();
        connection.Dispose();
    }

    public IModel CreateChannel()
    {
        if (connection is null || channelCount >= connection.ChannelMax) connection = factory.CreateConnection();

        channelCount++;

        return connection.CreateModel();
    }

    public IModel GetChannel()
    {
        if (channel is null) channel = CreateChannel();

        return channel;
    }
}