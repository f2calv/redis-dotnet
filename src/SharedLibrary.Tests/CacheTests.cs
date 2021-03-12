using CasCap.Extensions;
using CasCap.Models;
using CasCap.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
namespace CasCap.Tests
{
    public class CacheTests
    {
        readonly RedisCacheService _redisCacheSvc;

        public CacheTests()
        {
            var configuration = new ConfigurationBuilder()
                //.AddInMemoryCollection()
                .Build();

            //initiate ServiceCollection w/logging
            var services = new ServiceCollection()
                //.AddSingleton<IConfiguration>(configuration)
                .AddLogging(logging =>
                {
                    //    logging.AddDebug();
                    //    ApplicationLogging.LoggerFactory = logging.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
                })
                ;

            //todo: replace the below with IOptions?
            //_appConfig = configuration.GetSection(nameof(AppConfig)).Get<AppConfig>();

            //add services
            //services.AddSingleton(p => _appConfig);
            //IHostEnvironment env = new HostingEnvironment { EnvironmentName = Environments.Development };
            services.AddSingleton<RedisCacheService>();

            //assign services to be tested
            var _serviceProvider = services.BuildServiceProvider();
            _redisCacheSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        }

        [Theory]
        [InlineData("hello world!")]
        [InlineData("testing 123...")]
        public async Task dtString(string input)
        {
            var key = $"datatypes:{DateTime.UtcNow:yyyy-MM-dd}:string";
            await _redisCacheSvc.db.KeyDeleteAsync(key);

            var value = $"{input} {DateTime.UtcNow}";
            await _redisCacheSvc.db.StringSetAsync(key, value);
            var result = await _redisCacheSvc.db.StringGetAsync(key);
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Lets serialize an object to JSON, cache it, and then retrieve and deserialize.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("hello world!")]
        [InlineData("testing 123...")]
        public async Task dtStringJson(string input)
        {
            var key = $"datatypes:{DateTime.UtcNow:yyyy-MM-dd}:stringjson";
            await _redisCacheSvc.db.KeyDeleteAsync(key);

            var objOut = new MyObj($"{input} {DateTime.UtcNow}");
            var jsonOut = objOut.ToJSON();
            await _redisCacheSvc.db.StringSetAsync(key, jsonOut);
            var result = await _redisCacheSvc.db.StringGetAsync(key);
            Assert.Equal(jsonOut, result);
            Assert.Equal(objOut, result.FromJSON<MyObj>());
        }

        /// <summary>
        /// Lets serialize an object to MessagePack, cache it, and then retrieve and deserialize.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData("hello world!")]
        [InlineData("testing 123...")]
        public async Task dtStringMsgPack(string input)
        {
            var key = $"datatypes:{DateTime.UtcNow:yyyy-MM-dd}:stringmessagepack";
            await _redisCacheSvc.db.KeyDeleteAsync(key);

            var objOut = new MyObj($"{input} {DateTime.UtcNow}");
            var jsonOut = objOut.ToMessagePack();
            await _redisCacheSvc.db.StringSetAsync(key, jsonOut);
            var result = await _redisCacheSvc.db.StringGetAsync(key);
            Assert.Equal(jsonOut.Length, result.Length());
            Assert.Equal(objOut, result.FromMessagePack<MyObj>());
        }
    }
}