using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitWrapped.Configuration;
using RabbitWrapped.Example.Consumer;
using RabbitWrapped.Example.Message;
using RabbitWrapped.Extension;

namespace RabbitWrapped.Example;

public class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

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

    public void Configure(IApplicationBuilder app)
    {
    }
}