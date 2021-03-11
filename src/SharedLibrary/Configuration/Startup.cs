using CasCap.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
namespace CasCap.Configuration
{
    public class Startup
    {
        public static int Start(string[] args = null)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var result = 0;
            try
            {
                Log.Information("Starting {AppName}", AppDomain.CurrentDomain.FriendlyName);
                Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<RedisCacheService>();
                        services.AddHostedService<StreamingClientService>();
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
}