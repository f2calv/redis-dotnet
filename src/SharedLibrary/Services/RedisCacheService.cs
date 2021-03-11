using System;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
namespace CasCap.Services
{
    public class RedisCacheService
    {
        readonly ILogger _logger;

        public RedisCacheService(ILogger<RedisCacheService> logger)
        {
            _logger = logger;
            _configurationOptions = ConfigurationOptions.Parse("localhost:6379");//or add hosts entry for redis-svc->127.0.0.1
            _configurationOptions.ConnectRetry = 20;
            _configurationOptions.ClientName = Environment.MachineName;
            //Note: below for getting redis working container to container on docker compose, https://github.com/StackExchange/StackExchange.Redis/issues/1002
            //_configurationOptions.ResolveDns = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_COMPOSE"), out var _);

            //LuaScripts = GetLuaScripts();
        }

        static ConfigurationOptions _configurationOptions { get; set; } = new();

        readonly Lazy<ConnectionMultiplexer> LazyConnection = new(() => ConnectionMultiplexer.Connect(_configurationOptions));

        ConnectionMultiplexer Connection { get { return LazyConnection.Value; } }

        IDatabase _redis { get { return Connection.GetDatabase(); } }

        //IServer server { get { return Connection.GetServer(_configurationOptions.EndPoints[0]); } }

        public byte[] Get(string key) => _redis.StringGet(key);

        public bool Add(string key, string value, CommandFlags flags = CommandFlags.None)
        {
            _logger.LogTrace("Attempting to set redis cache key '{key}' to value '{value}'", key, value);
            var result = _redis.StringSet(key, value, flags: flags);
            return result;
        }

        public string? GetString(string key)
        {
            var val = _redis.StringGet(key);
            if (val.HasValue)
                return val;
            else
                return null;
        }
    }
}