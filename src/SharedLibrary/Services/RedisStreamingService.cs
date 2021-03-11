using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace CasCap.Services
{
    public class RedisStreamingService : BackgroundService
    {
        readonly ILogger<RedisStreamingService> _logger;
        readonly RedisCacheService _redisCacheSvc;

        public RedisStreamingService(ILogger<RedisStreamingService> logger, RedisCacheService redisCacheSvc)
        {
            _logger = logger;
            _redisCacheSvc = redisCacheSvc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //var message = $"hello at {DateTime.UtcNow}";
                //_logger.LogInformation("Message sent at {utcNow}, {message}", DateTime.UtcNow, message);

                var messageId = _redisCacheSvc._database.StreamAdd("event_stream", "foo_name", "bar_value");

                var values = new[]
                {
                    new NameValueEntry("sensor_id", "1234"),
                    new NameValueEntry("temp", "19.8")
                };
                var messageId2 = _redisCacheSvc._database.StreamAdd("sensor_stream", values);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}