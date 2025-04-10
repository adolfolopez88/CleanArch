namespace CleanArch.Domain.Models
{
    /// <summary>
    /// Represents a weather forecast for a specific date
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// The date of the forecast
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The temperature in Celsius
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// The temperature in Fahrenheit, calculated from Celsius
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// A textual summary of the weather conditions
        /// </summary>
        public string? Summary { get; set; }
    }

    /// <summary>
    /// Request model for creating a custom weather forecast
    /// </summary>
    public class WeatherForecastRequest
    {
        /// <summary>
        /// The city to get the forecast for
        /// </summary>
        public string? City { get; set; }
        
        /// <summary>
        /// The number of days to forecast (1-14)
        /// </summary>
        public int Days { get; set; } = 5;
        
        /// <summary>
        /// Whether to include detailed weather descriptions
        /// </summary>
        public bool IncludeDetails { get; set; } = true;
    }

    /// <summary>
    /// Response model containing weather forecast data
    /// </summary>
    public class WeatherForecastResponse
    {
        /// <summary>
        /// The city the forecast is for
        /// </summary>
        public string City { get; set; } = string.Empty;
        
        /// <summary>
        /// When the forecast was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// The collection of weather forecasts
        /// </summary>
        public List<WeatherForecast> Forecasts { get; set; } = new List<WeatherForecast>();
    }
}