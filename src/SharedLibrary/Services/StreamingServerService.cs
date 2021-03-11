using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace CasCap.Services
{
    public class StreamingServerService : BackgroundService
    {
        readonly ILogger<StreamingServerService> _logger;
        readonly RedisCacheService _redisCacheSvc;

        public StreamingServerService(ILogger<StreamingServerService> logger, RedisCacheService redisCacheSvc)
        {
            _logger = logger;
            _redisCacheSvc = redisCacheSvc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = $"hello at {DateTime.UtcNow}";
                _logger.LogInformation("Message sent at {utcNow}, {message}", DateTime.UtcNow, message);
                _redisCacheSvc._subscriber.Publish("messages", message);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}