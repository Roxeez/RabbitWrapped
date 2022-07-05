using RabbitWrapped.Example.Message;

namespace RabbitWrapped.Example.Consumer;

public class MyMessageConsumer : MessageConsumer<MyMessage>
{
    protected override void Consume(MyMessage message)
    {
        
    }
}