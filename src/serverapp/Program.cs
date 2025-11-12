using CasCap.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var result = 0;
try
{
    Log.Information("Starting {AppName}", AppDomain.CurrentDomain.FriendlyName);
    await Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddSingleton<IPriceGeneratorService, PriceGeneratorService>();
            services.AddSingleton<RedisCacheService>();
            services.AddHostedService<RedisPublisherService>();
        })
        .UseSerilog()
        .Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "{AppName} terminated unexpectedly", AppDomain.CurrentDomain.FriendlyName);
    result = 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
return result;
