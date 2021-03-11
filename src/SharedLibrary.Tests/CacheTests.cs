using CasCap.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
namespace CasCap.Tests
{
    public class CacheTests
    {
        readonly RedisCacheService _redisSvc;

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
            _redisSvc = _serviceProvider.GetRequiredService<RedisCacheService>();
        }

        //[Fact]
        //public void AddAndRetrieve()
        //{
        //    var key = $"mykey{Guid.NewGuid()}";
        //    var value = $"hello world! {DateTime.UtcNow}";
        //    _redisSvc.Set(key, value);

        //    var retrieve = _redisSvc.GetString(key);
        //    Assert.Equal(value, retrieve);
        //}
    }
}