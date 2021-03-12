using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using CasCap.Models;
using CasCap.Services;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace CasCap
{
    //https://benchmarkdotnet.org/articles/overview.html
    //[ClrJob, CoreJob]
    [/*SimpleJob(RuntimeMoniker.Net48), */SimpleJob(RuntimeMoniker.CoreRt50)]
    [MemoryDiagnoser]
    public class MyBenchmarks
    {
        //[InProcess]
        RedisCacheService _redisCacheSvc;

        [Params(10_000)]
        public int threshold { get; set; }

        [Params("GBPUSD")]
        public string symbol { get; set; }

        public List<Tick> ticks { get; set; } = new List<Tick>();

        [GlobalSetup]
        public void Setup()
        {
            var services = new ServiceCollection().AddLogging();
            services.AddSingleton<RedisCacheService>();
            var _serviceProvider = services.BuildServiceProvider();
            _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();

            var r = new Random();
            var l = new List<Tick>(threshold);
            var lastTick = new Tick(symbol, DateTime.UtcNow, 100.1, 100.2);
            for (var i = 0; i < threshold; i++)
            {
                //generate random price change
                var rDiff = Math.Round((r.NextDouble() * 2) - 1.0, 1);
                var tick = new Tick(symbol, DateTime.UtcNow, lastTick.Bid + rDiff, lastTick.Offer + rDiff);
                l.Add(tick);
                lastTick = tick;
            }
        }

        [Benchmark(Baseline = true)]
        public void StreamAdd()
        {
            //foreach (var tick in ticks)
            //    _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, tick.Symbol, tick.Bid);
        }

        //[Benchmark]
        //public void StreamAddFireAndForget()
        //{
        //    foreach (var tick in ticks)
        //        _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, tick.Symbol, tick.Bid, flags: CommandFlags.FireAndForget);
        //}

        //[Benchmark]
        //public async Task StreamAddAsync()
        //{
        //    foreach (var tick in ticks)
        //        _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, tick.Symbol, tick.Bid);
        //}

        //[Benchmark]
        //public async Task StreamAddAsyncFireAndForget()
        //{
        //    foreach (var tick in ticks)
        //        _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, tick.Symbol, tick.Bid, flags: CommandFlags.FireAndForget);
        //}

        [GlobalCleanup]
        public void Cleanup()
        {

        }
    }
}