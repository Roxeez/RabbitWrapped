using System;

namespace RabbitWrapped;

public interface IMessageConsumer
{
    Type MessageType { get; }
    void Consume(IMessage message);
}

public abstract class MessageConsumer<T> : IMessageConsumer where T : IMessage
{
    public Type MessageType { get; } = typeof(T);

    public void Consume(IMessage message)
    {
        Consume((T) message);
    }

    protected abstract void Consume(T message);
}

