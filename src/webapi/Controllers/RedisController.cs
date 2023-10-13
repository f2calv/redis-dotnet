using CasCap.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
namespace CasCap.Controllers;

[ApiController]
[Route("[controller]")]
public class RedisController : ControllerBase
{
    readonly ILogger<RedisController> _logger;
    readonly RedisCacheService _redisCacheSvc;

    public RedisController(ILogger<RedisController> logger, RedisCacheService redisCacheSvc)
    {
        _logger = logger;
        _redisCacheSvc = redisCacheSvc;
    }

    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
        //https://redis.io/topics/data-types-intro

        var prefix = nameof(RedisController);

        var val = DateTime.UtcNow.Ticks.ToString();

        //Lists: collections of string elements sorted according to the order of insertion. They are basically linked lists.
        await _redisCacheSvc.db.ListLeftPushAsync($"{prefix}:list", val, flags: CommandFlags.FireAndForget);
        await _redisCacheSvc.db.ListLeftPushAsync($"{prefix}:list", val, flags: CommandFlags.FireAndForget);//add twice to show additional insertion

        await _redisCacheSvc.db.KeyExpireAsync($"{prefix}:list", TimeSpan.FromMinutes(5), flags: CommandFlags.FireAndForget);//lets add an expiry later...

        //Sets: collections of unique, unsorted string elements.
        await _redisCacheSvc.db.SetAddAsync($"{prefix}:set", val, flags: CommandFlags.FireAndForget);
        await _redisCacheSvc.db.SetAddAsync($"{prefix}:set", val, flags: CommandFlags.FireAndForget);//add twice to show NO additional insertion

        //Sorted sets, similar to Sets but where every string element is associated to a floating number value, called score.
        //The elements are always taken sorted by their score, so unlike Sets it is possible to retrieve a range of elements
        //(for example you may ask: give me the top 10, or the bottom 10).
        await _redisCacheSvc.db.SortedSetAddAsync($"{prefix}:sortedset", "fred's high score", DateTime.UtcNow.Minute, flags: CommandFlags.FireAndForget);
        await _redisCacheSvc.db.SortedSetAddAsync($"{prefix}:sortedset", "fred's high score", DateTime.UtcNow.Minute * 2, flags: CommandFlags.FireAndForget);//added twice to show NO additional insertion but diff score

        await _redisCacheSvc.db.SortedSetAddAsync($"{prefix}:sortedset", "barney's high score", 100, flags: CommandFlags.FireAndForget);
        await _redisCacheSvc.db.SortedSetIncrementAsync($"{prefix}:sortedset", "barney's high score", 1, flags: CommandFlags.FireAndForget);//+1 to the high score

        //Hashes, which are maps composed of fields associated with values. Both the field and the value are strings. This is very similar to Ruby or Python hashes.
        //await _redisCacheSvc.db.HashSetAsync($"{prefix}:hash")
        await _redisCacheSvc.db.HashSetAsync($"{prefix}:hash",
            new HashEntry[]
            {
                new HashEntry("firstname", "fred"),
                new HashEntry("surname", "bloggs"),
                new HashEntry("age", "21")
            },
            flags: CommandFlags.FireAndForget);
        await _redisCacheSvc.db.HashIncrementAsync($"{prefix}:hash", "age", 2, flags: CommandFlags.FireAndForget);//add 2 to the age value
        await _redisCacheSvc.db.HashIncrementAsync($"{prefix}:hash", "firstname", 1, flags: CommandFlags.FireAndForget);//add 1 to the name value - will fail but return no error due to FireAndForget!

        //other data types;
        //Bit arrays
        //HyperLogLogs
        //Streams - see other apps in sln...

        return Ok("all good!");
    }
}