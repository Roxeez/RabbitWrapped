using Microsoft.Extensions.Logging;
using RabbitWrapped.Example.Message;

namespace RabbitWrapped.Example.Consumer;

public class MyMessageConsumer : MessageConsumer<MyMessage>
{
    private readonly ILogger<MyMessageConsumer> logger;

    public MyMessageConsumer(ILogger<MyMessageConsumer> logger)
    {
        this.logger = logger;
    }

    protected override void Consume(MyMessage message)
    {
        logger.LogInformation("Username: {username}", message.Username);
    }
}