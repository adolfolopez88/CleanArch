using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArch.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class SampleController : ControllerBase
    {
        private readonly ILogger<SampleController> _logger;

        public SampleController(ILogger<SampleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a sample response
        /// </summary>
        /// <returns>A sample response object</returns>
        /// <response code="200">Returns the sample object</response>
        [HttpGet]
        [ProducesResponseType(typeof(SampleResponse), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            _logger.LogInformation("Sample API endpoint called");
            
            return Ok(new SampleResponse
            {
                Message = "API is working correctly",
                Timestamp = DateTimeOffset.UtcNow
            });
        }

        /// <summary>
        /// Protected endpoint that requires authentication
        /// </summary>
        /// <returns>A sample response with user information</returns>
        /// <response code="200">Returns the sample object</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet("protected")]
        [Authorize]
        [ProducesResponseType(typeof(SampleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetProtected()
        {
            var userId = User.Identity?.Name ?? "unknown";
            _logger.LogInformation("Protected endpoint called by user {UserId}", userId);
            
            return Ok(new SampleResponse
            {
                Message = $"Hello, {userId}! This is a protected endpoint.",
                Timestamp = DateTimeOffset.UtcNow
            });
        }
    }

    public class SampleResponse
    {
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
    }
}