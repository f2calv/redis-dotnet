namespace CasCap.Configuration;

public class Startup
{
    public static int Start()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var result = 0;
        try
        {
            Log.Information("Starting {AppName}", AppDomain.CurrentDomain.FriendlyName);
            Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<RedisCacheService>();
                    services.AddHostedService<RedisSubscriberService>();
                    //services.AddHostedService<RedisConsumerService>();
                })
                .UseSerilog()
                .Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "{AppName} terminated unexpectedly", AppDomain.CurrentDomain.FriendlyName);
            result = 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
        return result;
    }
}