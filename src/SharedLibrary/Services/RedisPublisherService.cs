using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace CasCap.Services
{
    public class RedisPublisherService : BackgroundService
    {
        readonly ILogger<RedisPublisherService> _logger;
        readonly RedisCacheService _redisCacheSvc;

        public RedisPublisherService(ILogger<RedisPublisherService> logger, RedisCacheService redisCacheSvc)
        {
            _logger = logger;
            _redisCacheSvc = redisCacheSvc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var counter = 0L;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (false)
                {
                    var message = $"hello at {DateTime.UtcNow}";
                    _logger.LogInformation("Message sent at {utcNow}, {message}", DateTime.UtcNow, message);
                    _redisCacheSvc._subscriber.Publish("messages", message);
                    await Task.Delay(1000, stoppingToken);
                }
                else
                {
                    //var message = $"hello at {DateTime.UtcNow}";
                    //_logger.LogInformation("Message sent at {utcNow}, {message}", DateTime.UtcNow, message);

                    //var messageId = _redisCacheSvc.db.StreamAdd(streamKey, "foo_name", "bar_value");

                    //var values = new[]
                    //{
                    //    new NameValueEntry("sensor_id", "1234"),
                    //    new NameValueEntry("temp", "19.8")
                    //};
                    //var messageId2 = _redisCacheSvc.db.StreamAdd("sensor_stream", values);

                    //_logger.LogInformation("Streams updated at {utcNow}", DateTime.UtcNow);
                    //await Task.Delay(10, stoppingToken);

                    //which is fastest?
                    _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, "GBPUSD", 1.5456);

                    _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, "GBPUSD", 1.5456, flags: CommandFlags.FireAndForget);

                    _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, "GBPUSD", 1.5456);

                    _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, "GBPUSD", 1.5456, flags: CommandFlags.FireAndForget);
                }
                counter++;
                if (counter % 1000 == 0)
                    _logger.LogInformation("{counter} records at {utcNow}", counter, DateTime.UtcNow);
                if (counter % 1_000_000 == 0)
                    break;

                //todo: pass a strong-typed Tick object
                void Test()
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                    }
                }
            }
        }
    }
}