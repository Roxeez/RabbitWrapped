# RabbitWrapped

### Producer
```c#
public class WorkerService : BackgroundService
{
    private readonly MessageProducer<MyMessage> producer;

    public WorkerService(MessageProducer<MyMessage> producer)
    {
        this.producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            producer.Produce(new MyMessage
            {
                Username = "test"
            });
        }
    }
}
```

### Consumer
```c#
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
```

### Configuration

#### With appsettings
``` json
{
  "Rabbit": {
    "Host": "127.0.0.1",
    "Username": "guest",
    "Password": "guest",
    "Port": 5672
  }
}
```
```c#
private readonly IConfiguration configuration;

public Startup(IConfiguration configuration)
{
    this.configuration = configuration;
}

public void ConfigureServices(IServiceCollection services)
{
    services.AddRabbit(configuration);
    
    services.AddMessageProducer<MyMessage>(queue: "my-queue");
    services.AddMessageConsumer<MyMessageConsumer>(queue: "my-queue");
}
```
#### Without appsettings
```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddRabbit(new RabbitConfiguration
    {
        Host = "127.0.0.1",
        Port = 5672,
        Username = "guest",
        Password = "guest"
    });
    
    services.AddMessageProducer<MyMessage>(queue: "my-queue");
    services.AddMessageConsumer<MyMessageConsumer>(queue: "my-queue");
}
```
