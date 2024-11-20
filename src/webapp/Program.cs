using CasCap;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var result = 0;
try
{
    Log.Information("Starting {AppName}", AppDomain.CurrentDomain.FriendlyName);
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
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