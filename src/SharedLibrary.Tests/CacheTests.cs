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
            _ = await _redisCacheSvc.db.KeyDeleteAsync(key);

            var value = $"{input} {DateTime.UtcNow}";
            _ = await _redisCacheSvc.db.StringSetAsync(key, value);
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
            _ = await _redisCacheSvc.db.KeyDeleteAsync(key);

            var objOut = new MyObj($"{input} {DateTime.UtcNow}");
            var jsonOut = objOut.ToJson();
            _ = await _redisCacheSvc.db.StringSetAsync(key, jsonOut);
            var result = await _redisCacheSvc.db.StringGetAsync(key);
            Assert.Equal(jsonOut, result);
            Assert.Equal(objOut, result.ToString().FromJson<MyObj>());
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
            _ = await _redisCacheSvc.db.KeyDeleteAsync(key);

            var objOut = new MyObj($"{input} {DateTime.UtcNow}");
            var jsonOut = objOut.ToMessagePack();
            _ = await _redisCacheSvc.db.StringSetAsync(key, jsonOut);
            byte[] result = await _redisCacheSvc.db.StringGetAsync(key);
            Assert.Equal(jsonOut.Length, result.Length);
            Assert.Equal(objOut, result.FromMessagePack<MyObj>());
        }

        [Theory]
        [InlineData("hello", "new", "world!")]
        [InlineData("apples", "oranges", "bananas!")]
        public async Task dtList(string a, string b, string c)
        {
            var key = $"datatypes:{DateTime.UtcNow:yyyy-MM-dd}:list";
            _ = await _redisCacheSvc.db.KeyDeleteAsync(key);

            //var objOut = new MyObj($"{input} {DateTime.UtcNow}");
            //var jsonOut = objOut.ToMessagePack();
            _ = await _redisCacheSvc.db.ListLeftPushAsync(key,
                new StackExchange.Redis.RedisValue[] {
                    new StackExchange.Redis.RedisValue(a),
                    new StackExchange.Redis.RedisValue(b),
                    new StackExchange.Redis.RedisValue(c)
                },
                StackExchange.Redis.CommandFlags.FireAndForget);
            var resulta = await _redisCacheSvc.db.ListRightPopAsync(key);
            Assert.Equal(resulta, a);
            var resultb = await _redisCacheSvc.db.ListLeftPopAsync(key);
            Assert.Equal(resultb, c);//swapped
            var resultc = await _redisCacheSvc.db.ListGetByIndexAsync(key, 0);
            Assert.Equal(resultc, b);//swapped!
        }

        [Fact]
        public async Task dtHashSet_Exception()
        {
            await _redisCacheSvc.db.HashSetAsync("prefix:key", new HashEntry[] { new HashEntry("hashField", "abc") });
            var before = await _redisCacheSvc.db.HashGetAsync("prefix:key", "hashField");
            Assert.Equal("abc", before);
            Func<Task> after = () => _redisCacheSvc.db.HashIncrementAsync("prefix:key", "hashField", 1);
            var exception = await Assert.ThrowsAsync<RedisServerException>(after);
            Debug.WriteLine(exception.Message);
        }
    }
}