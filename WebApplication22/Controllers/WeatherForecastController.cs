using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication22.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private TelemetryClient telemetry;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, TelemetryClient t)
        {
            _logger = logger;
            telemetry = t;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            //Makes the headers configurable
            Activity.Current = null;

            var operationId = "CR" + Guid.NewGuid().ToString();

            //setup telemetry client
            telemetry.Context.Operation.Id = operationId;
            if (!telemetry.Context.GlobalProperties.ContainsKey("keep"))
            {
                telemetry.Context.GlobalProperties.Add("keep", "true");
            }

            var url = "https:microsoft.com";
            using (var client = new HttpClient())
            {
                using (var requestMessage =
                    new HttpRequestMessage(HttpMethod.Get, url))
                {
                    //set header manually
                    requestMessage.Headers.Add("Request-Id", operationId);
                    await client.SendAsync(requestMessage);
                }
            }

            //send custom telemetry 
            telemetry.TrackDependency("Http", url, "myCall", DateTime.Now, new TimeSpan(0, 0, 0, 1), true);



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
