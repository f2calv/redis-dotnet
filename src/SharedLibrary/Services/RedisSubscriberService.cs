using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace CasCap.Services;

public class RedisSubscriberService : BackgroundService
{
    readonly ILogger<RedisSubscriberService> _logger;
    readonly RedisCacheService _redisCacheSvc;

    public RedisSubscriberService(ILogger<RedisSubscriberService> logger, RedisCacheService redisCacheSvc)
    {
        _logger = logger;
        _redisCacheSvc = redisCacheSvc;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(0);
        if (false)
        {
            //// Synchronous handler
            //_redisCacheSvc._subscriber.Subscribe("messages").OnMessage(channelMessage => {
            //    Console.WriteLine((string)channelMessage.Message);
            //});

            // Asynchronous handler
            _redisCacheSvc.subscriber.Subscribe("messages").OnMessage(async channelMessage =>
            {
                await Task.Delay(0);
                _logger.LogInformation("Message received at {utcNow}, {message}", DateTime.UtcNow, (string)channelMessage.Message);
            });
        }
        else
        {
            var position = "0-0";
            var batchSize = 100;
            var info = await _redisCacheSvc.db.StreamInfoAsync(Globals.streamKey);
            _logger.LogInformation("stream {streamName} information is {@streamInfo}", Globals.streamKey, info);

            if (false)
            {
                //read a chunk from the stream
                var streamEntriesBatch = await _redisCacheSvc.db.StreamReadAsync(Globals.streamKey, position, count: 1_000_000);
                var counter = 0;
                foreach (var streamEntry in streamEntriesBatch)
                {
                    if (counter % 1000 == 0)
                        _logger.LogInformation("stream entry {id}, data {Name}:{Value}", streamEntry.Id, streamEntry.Values[0].Name, streamEntry.Values[0].Value);
                }
            }

            if (true)
            {
                //read in batches from the stream using either;
                //  a) StreamReadAsync
                //  b) StreamRangeAsync
                //and optionally deleting the messages after processing
                var iteration = 0;
                StreamEntry[] streamEntriesBatch;
                do
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;
                    var startPosition = position;
                    var messageIds = new List<RedisValue>(batchSize);
                    //streamEntriesBatch = await _redisCacheSvc.db.StreamReadAsync(streamKey, position, batchSize);
                    streamEntriesBatch = await _redisCacheSvc.db.StreamRangeAsync(Globals.streamKey, minId: position, maxId: "+", count: batchSize);
                    foreach (var streamEntry in streamEntriesBatch)
                    {
                        foreach (var streamEntryValue in streamEntry.Values)
                            _logger.LogDebug("iteration {iteration}, stream entry {Id}, data {Name}:{Value}", iteration, streamEntry.Id, streamEntryValue.Name, streamEntryValue.Value);
                        messageIds.Add(streamEntry.Id);
                        position = streamEntry.Id;
                    }
                    var deletedCount = await _redisCacheSvc.db.StreamDeleteAsync(Globals.streamKey, messageIds.ToArray());
                    _logger.LogInformation("{deletedCount} stream entries deleted, from {startPosition} -> {endPosition}",
                        deletedCount, startPosition, position);
                    iteration++;
                } while (info.LastEntry.Id != position);
            }
        }
    }
}