using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddRabbit(configuration);
        services.AddMessageProducer<MyMessage>(queue: "my-queue");
        services.AddMessageConsumer<MyMessageConsumer>(queue: "my-queue");

        services.AddHostedService<WorkerService>();
    }

    public void Configure(IApplicationBuilder app)
    {
    }
}