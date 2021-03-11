using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace CasCap.Services
{
    public class StreamingClientService : BackgroundService
    {
        readonly ILogger<StreamingClientService> _logger;
        readonly RedisCacheService _redisCacheSvc;

        public StreamingClientService(ILogger<StreamingClientService> logger, RedisCacheService redisCacheSvc)
        {
            _logger = logger;
            _redisCacheSvc = redisCacheSvc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(0);
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {utcNow}", DateTime.UtcNow);
            //    await Task.Delay(1000, stoppingToken);
            //}

            //// Synchronous handler
            //_redisCacheSvc._subscriber.Subscribe("messages").OnMessage(channelMessage => {
            //    Console.WriteLine((string)channelMessage.Message);
            //});

            // Asynchronous handler
            _redisCacheSvc._subscriber.Subscribe("messages").OnMessage(async channelMessage => {
                //await Task.Delay(1000);
                _logger.LogInformation("Message received at {utcNow}, {message}", DateTime.UtcNow, (string)channelMessage.Message);
            });
        }
    }
}