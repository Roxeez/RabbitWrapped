using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RabbitWrapped.Example;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(x =>
    {
        x.UseStartup<Startup>();
    })
    .UseConsoleLifetime()
    .Build();

using (host)
{
    await host.StartAsync();
    await host.WaitForShutdownAsync();
}