using CasCap.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
namespace CasCap.Services
{
    public class RedisCacheService
    {
        readonly ILogger _logger;

        public RedisCacheService(ILogger<RedisCacheService> logger)
        {
            _logger = logger;
            configuration = ConfigurationOptions.Parse("localhost:6379");
            configuration.ConnectRetry = 20;
            configuration.ClientName = $"{AppDomain.CurrentDomain.FriendlyName}-{Environment.MachineName}";
            //configuration.AllowAdmin = true;
            //Note: below for getting redis working container to container on docker compose, https://github.com/StackExchange/StackExchange.Redis/issues/1002
            //_configurationOptions.ResolveDns = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_COMPOSE"), out var _);

            //LuaScripts = GetLuaScripts();
        }

        static ConfigurationOptions configuration { get; set; } = new();

        readonly Lazy<ConnectionMultiplexer> LazyConnection = new(() => ConnectionMultiplexer.Connect(configuration));

        ConnectionMultiplexer Connection { get { return LazyConnection.Value; } }

        public IDatabase db { get { return Connection.GetDatabase(); } }

        public ISubscriber subscriber { get { return Connection.GetSubscriber(); } }

        public IServer server { get { return Connection.GetServer(configuration.EndPoints[0]); } }

        public bool Set<T>(string key, T value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            _logger.LogTrace("Attempting to set redis cache key '{key}' to value '{value}' (expiry is {expiry})", key, value, expiry);
            var result = db.StringSet(key, value?.ToJSON(), expiry, flags: flags);
            return result;
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            _logger.LogTrace("Attempting to set redis cache key '{key}' to value '{value}' (expiry is {expiry})", key, value, expiry);
            var result = await db.StringSetAsync(key, value?.ToJSON(), expiry, flags: flags);
            return result;
        }

        public byte[] Get(string key) => db.StringGet(key);

        public async Task<byte[]> GetAsync(string key) => await db.StringGetAsync(key);

        public T? Get<T>(string key)
        {
            var val = db.StringGet(key);
            if (val.HasValue)
                return ((string)val).FromJSON<T>();
            else
                return default(T);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var val = await db.StringGetAsync(key);
            if (val.HasValue)
                return ((string)val).FromJSON<T>();
            else
                return default(T);
        }

        bool Set(string key, string value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            _logger.LogTrace("Attempting to set redis cache key '{key}' to value '{value}' (expiry is {expiry})", key, value, expiry);
            //note: String is a Redis string type, not exactly a .NET string type!
            var result = db.StringSet(key, value, expiry, flags: flags);
            return result;
        }

        async Task<bool> SetAsync(string key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            var result = await db.StringSetAsync(key, value, expiry, flags: flags);
            return result;
        }

        string? GetString(string key)
        {
            var val = db.StringGet(key);
            if (val.HasValue)
                return (string)val;
            else
                return null;
        }

        byte[]? GetBytes(string key)
        {
            var val = db.StringGet(key);
            if (val.HasValue)
                return (byte[])val;
            else
                return null;
        }
    }
}