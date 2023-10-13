using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using CasCap.Extensions;
using CasCap.Models;
using CasCap.Services;
using Microsoft.Extensions.DependencyInjection;
using Perfolizer.Horology;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace CasCap;

//https://benchmarkdotnet.org/articles/overview.html
//[ClrJob, CoreJob]
//[SimpleJob(RuntimeMoniker.CoreRt31, warmupCount: 5, targetCount: 5, baseline: true)]
//[SimpleJob(RuntimeMoniker.Net50, warmupCount: 5, targetCount: 5)]
[SimpleJob(RuntimeMoniker.Net60)]
//[MemoryDiagnoser]
[Config(typeof(FastAndDirtyConfig))]
public class MyBenchmarks
{
    class FastAndDirtyConfig : ManualConfig
    {
        //https://fransbouma.github.io/BenchmarkDotNet/faq.htm
        public FastAndDirtyConfig()
        {
            AddJob(Job.Default
                .WithLaunchCount(1)     // benchmark process will be launched only once
                .WithIterationTime(new TimeInterval(100, TimeUnit.Millisecond)) // 100ms per iteration
                .WithWarmupCount(1)     // 1 warmup iteration
            );
        }
    }

    [Params(100)]
    public int maxIterations { get; set; }

    [Params("GBPUSD")]
    public string symbol { get; set; }

    public List<Tick> ticks { get; set; } = new List<Tick>();

    //[InProcess]
    ServiceProvider _serviceProvider;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<RedisCacheService>();
        _serviceProvider = services.BuildServiceProvider();

        var r = new Random();
        var l = new List<Tick>(maxIterations);
        var lastTick = new Tick(symbol, DateTime.UtcNow, 100.1, 100.2);
        for (var i = 0; i < maxIterations; i++)
        {
            //generate random price change
            var rDiff = Math.Round((r.NextDouble() * 2) - 1.0, 1);
            var tick = new Tick(symbol, DateTime.UtcNow, lastTick.Bid + rDiff, lastTick.Offer + rDiff);
            l.Add(tick);
            lastTick = tick;
        }
    }

    /*
    [Benchmark(Baseline = true)]
    public void StreamAdd()
    {
        var _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        foreach (var tick in ticks)
            _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, tick.Symbol, tick.Bid);
    }

    [Benchmark]
    public void StreamAddFireAndForget()
    {
        var _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        foreach (var tick in ticks)
            _ = _redisCacheSvc.db.StreamAdd(Globals.streamKey, tick.Symbol, tick.Bid, flags: CommandFlags.FireAndForget);
    }

    [Benchmark]
    public async Task StreamAddAsync()
    {
        var _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        foreach (var tick in ticks)
            _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, tick.Symbol, tick.Bid);
    }

    [Benchmark]
    public async Task StreamAddAsyncFireAndForget()
    {
        var _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        foreach (var tick in ticks)
            _ = await _redisCacheSvc.db.StreamAddAsync(Globals.streamKey, tick.Symbol, tick.Bid, flags: CommandFlags.FireAndForget);
    }
    */

    [Benchmark]
    public async Task DataTypeString()
    {
        var _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        var key = $"datatypes:{DateTime.UtcNow:yyyy-MM-dd}:string";
        var value = $"hello world! {DateTime.UtcNow}";
        for (var i = 0; i < 10_000; i++)
        {
            _ = await _redisCacheSvc.db.KeyDeleteAsync(key);
            _ = await _redisCacheSvc.db.StringSetAsync(key, value);
            _ = await _redisCacheSvc.db.StringGetAsync(key);
        }
    }

    [Benchmark(OperationsPerInvoke = 1)]
    public async Task DataTypeJson()
    {
        var _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        var key = $"datatypes:{DateTime.UtcNow:yyyy-MM-dd}:stringjson";
        var obj = new MyObj($"hello world! {DateTime.UtcNow}");
        for (var i = 0; i < maxIterations; i++)
        {
            _ = await _redisCacheSvc.db.KeyDeleteAsync(key);
            _ = await _redisCacheSvc.db.StringSetAsync(key, obj.ToJSON());
            var result = await _redisCacheSvc.db.StringGetAsync(key);
            _ = result.FromJSON<MyObj>();
        }
    }

    [Benchmark]
    public async Task DataTypeMessagePack()
    {
        var _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        var key = $"datatypes:{DateTime.UtcNow:yyyy-MM-dd}:stringmessagepack";
        var obj = new MyObj($"hello world! {DateTime.UtcNow}");
        for (var i = 0; i < 10_000; i++)
        {
            _ = await _redisCacheSvc.db.KeyDeleteAsync(key);
            _ = await _redisCacheSvc.db.StringSetAsync(key, obj.ToMessagePack());
            var result = await _redisCacheSvc.db.StringGetAsync(key);
            _ = result.FromMessagePack<MyObj>();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {

    }
}