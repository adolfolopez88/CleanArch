using CleanArch.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.WebApi.Controllers
{
    /// <summary>
    /// Provides weather forecast data for demonstration purposes
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class WeatherController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherController> _logger;

        public WeatherController(ILogger<WeatherController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get a list of weather forecasts for the next 5 days
        /// </summary>
        /// <returns>A collection of weather forecasts</returns>
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Getting weather forecast data");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// Get a weather forecast for a specific date
        /// </summary>
        /// <param name="date">The date to get the forecast for</param>
        /// <returns>A single weather forecast</returns>
        [HttpGet("{date}", Name = "GetWeatherForecastByDate")]
        public ActionResult<WeatherForecast> GetByDate(DateTime date)
        {
            _logger.LogInformation("Getting weather forecast for {Date}", date);

            var forecast = new WeatherForecast
            {
                Date = date,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };

            return Ok(forecast);
        }

        /// <summary>
        /// Get a detailed weather forecast for a specific city
        /// </summary>
        /// <param name="city">The city name</param>
        /// <param name="days">Number of days to forecast (optional, default is 5)</param>
        /// <returns>A weather forecast response</returns>
        [HttpGet("city/{city}", Name = "GetWeatherForecastByCity")]
        public ActionResult<WeatherForecastResponse> GetByCity(string city, [FromQuery] int days = 5)
        {
            _logger.LogInformation("Getting weather forecast for {City} for {Days} days", city, days);

            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City name is required");
            }

            if (days <= 0 || days > 14)
            {
                return BadRequest("Days must be between 1 and 14");
            }

            var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });

            var response = new WeatherForecastResponse
            {
                City = city,
                GeneratedAt = DateTime.UtcNow,
                Forecasts = forecasts.ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Create a custom weather forecast request
        /// </summary>
        /// <param name="request">The forecast request details</param>
        /// <returns>A customized weather forecast</returns>
        [HttpPost("forecast", Name = "CreateWeatherForecast")]
        public ActionResult<WeatherForecastResponse> CreateForecast([FromBody] WeatherForecastRequest request)
        {
            _logger.LogInformation("Creating custom weather forecast for {City} for {Days} days", request.City, request.Days);

            if (string.IsNullOrWhiteSpace(request.City))
            {
                return BadRequest("City name is required");
            }

            if (request.Days <= 0 || request.Days > 14)
            {
                return BadRequest("Days must be between 1 and 14");
            }

            var forecasts = Enumerable.Range(1, request.Days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = request.IncludeDetails ? Summaries[Random.Shared.Next(Summaries.Length)] : null
            });

            var response = new WeatherForecastResponse
            {
                City = request.City,
                GeneratedAt = DateTime.UtcNow,
                Forecasts = forecasts.ToList()
            };

            return CreatedAtAction(nameof(GetByCity), new { city = request.City, days = request.Days }, response);
        }

        /// <summary>
        /// Update a weather forecast
        /// </summary>
        /// <param name="id">The forecast ID</param>
        /// <param name="forecast">The updated forecast data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}", Name = "UpdateWeatherForecast")]
        [Authorize]
        public ActionResult UpdateForecast(int id, [FromBody] WeatherForecast forecast)
        {
            _logger.LogInformation("Updating weather forecast with ID {Id}", id);

            // In a real app, we would update the forecast in a database
            // For this demo, we'll just return success

            return NoContent();
        }

        /// <summary>
        /// Delete a weather forecast
        /// </summary>
        /// <param name="id">The forecast ID to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}", Name = "DeleteWeatherForecast")]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteForecast(int id)
        {
            _logger.LogInformation("Deleting weather forecast with ID {Id}", id);

            // In a real app, we would delete the forecast from a database
            // For this demo, we'll just return success

            return NoContent();
        }
        
        /// <summary>
        /// Get the current user's weather preferences
        /// </summary>
        /// <returns>Weather preferences for the authenticated user</returns>
        [HttpGet("preferences", Name = "GetWeatherPreferences")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult GetUserPreferences()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
            
            // In a real app, we would fetch user preferences from a database
            // For this demo, we'll return some mock data
            
            return Ok(new
            {
                UserId = userId,
                Email = email,
                Roles = roles,
                Preferences = new
                {
                    DefaultCity = "New York",
                    TemperatureUnit = "Celsius",
                    ShowHumidity = true,
                    ShowWindSpeed = true,
                    ForecastDays = 5
                }
            });
        }
    }
}