using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasstransitWebApiTemplate.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("这是测试信息....");
            var rng = new Random();
            var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = "123456789",
                Summary2 = "123456789",
                Summary3 = "123456789"
            })
            .ToArray();

            //当result为数组时，需要让params object[] 知道result是一个参数，多传一个参数 null 即可解决
            //或者使用 new[]{ result } 的方式传参
            _logger.LogInformation("执行Get的结果为1：{@result}", result);
            _logger.LogInformation("执行Get的结果为2：{@result}", result, null);

            return result;
        }
    }
}
