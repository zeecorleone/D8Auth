using D8Auth.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace D8Auth.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("admin-only")]
        [Authorize(Roles = Constants.Roles.Admin)]
        public async Task<IActionResult> AdminOnly()
        {
            return Ok (new { Message = "Hello, Admin! You have access to this endpoint." });
        }

        [HttpPost("manager-or-admin")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManagerOrAdmin()
        {
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            return Ok(new
            {
                Message = "Welcom Manager/Admin!",
                Roles = roles
            });
        }
    }
}
