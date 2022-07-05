using System;
using RabbitMQ.Client;
using RabbitWrapped.Configuration;

namespace RabbitWrapped.Common;

public class RabbitConnection : IDisposable
{
    private readonly ConnectionFactory factory;

    private IConnection connection;
    private IModel channel;
    private int channelCount;

    public RabbitConnection(RabbitConfiguration configuration)
    {
        factory = new ConnectionFactory
        {
            HostName = configuration.Host,
            UserName = configuration.Username,
            Password = configuration.Password,
            Port = configuration.Port
        };
    }
    
    public IModel CreateChannel()
    {
        if (connection is null ||  channelCount >= connection.ChannelMax)
        {
            connection = factory.CreateConnection();
        }

        channelCount++;
        
        return connection.CreateModel();
    }

    public IModel GetChannel()
    {
        if (channel is null)
        {
            channel = CreateChannel();
        }

        return channel;
    }

    public void Dispose()
    {
        channel.Dispose();
        connection.Dispose();
    }
}