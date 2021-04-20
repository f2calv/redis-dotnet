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
        public IPriceGeneratorService _priceGeneratorSvc;

        public RedisPublisherService(ILogger<RedisPublisherService> logger, RedisCacheService redisCacheSvc, IPriceGeneratorService priceGeneratorSvc)
        {
            _logger = logger;
            _redisCacheSvc = redisCacheSvc;
            _priceGeneratorSvc = priceGeneratorSvc;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var counter = 0L;

            await foreach (var price in _priceGeneratorSvc.GetPricesAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                if (false)
                {
                    var message = $"{price.symbol} is {price.bid}/{price.offer} at {price.date}";
                    _logger.LogInformation("latest {symol} price sent at {utcNow}, {message}", price.symbol, DateTime.UtcNow, message);
                    _redisCacheSvc.subscriber.Publish("messages", message);
                    await Task.Delay(1000, cancellationToken);
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
                    _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, nameof(price.symbol), price.symbol);

                    _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, nameof(price.symbol), price.symbol, flags: CommandFlags.FireAndForget);

                    _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, nameof(price.symbol), price.symbol);

                    var streamPairs = new NameValueEntry[3]
                    {
                        new NameValueEntry("s", price.symbol),
                        new NameValueEntry("b", price.bid),
                        new NameValueEntry("a", price.offer)
                    };
                    _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, streamPairs, flags: CommandFlags.FireAndForget);
                }
                counter++;
                if (counter % 1000 == 0)
                    _logger.LogInformation("{counter} records at {utcNow}", counter, DateTime.UtcNow);
                if (counter % 1_000_000 == 0)
                    break;
            }
        }
    }
}