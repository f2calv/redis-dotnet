using CasCap.Models;
using CasCap.Services;
using Microsoft.AspNetCore.Mvc;

namespace CasCap.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    readonly ILogger<WeatherForecastController> _logger;
    readonly RedisCacheService _redisCacheSvc;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, RedisCacheService redisCacheSvc)
    {
        _logger = logger;
        _redisCacheSvc = redisCacheSvc;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        var key = $"{nameof(WeatherForecastController)}:{nameof(Get)}";
        _logger.LogDebug("attempting to retrieve weather information using cache key {key}", key);

        //cache-aside pattern
        var weather = _redisCacheSvc.Get<WeatherForecast[]>(key);
        if (weather is null || weather.Length == 0)
        {
            _logger.LogWarning("cache item with cache key {key} doesn't exist", key);
            weather = GenerateWeather();
            _redisCacheSvc.Set(key, weather, TimeSpan.FromSeconds(10));//cache for n seconds
            _logger.LogInformation("item added with cache key {key} and data {data}", key, weather);
        }
        else
            _logger.LogInformation("cache item with cache key {key} retrieved!", key);
        return weather;

        WeatherForecast[] GenerateWeather()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}