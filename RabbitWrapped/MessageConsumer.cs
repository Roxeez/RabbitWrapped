using System;

namespace RabbitWrapped;

public abstract class MessageConsumer
{
    public abstract Type MessageType { get; }
    public abstract void Consume(IMessage message);
}

public abstract class MessageConsumer<T> : MessageConsumer where T : IMessage
{
    public override Type MessageType { get; } = typeof(T);

    public override void Consume(IMessage message)
    {
        Consume((T)message);
    }

    protected abstract void Consume(T message);
}