using Amazon.S3;
using DocuAI.Infrastructure.AWS.SDK;
using Microsoft.AspNetCore.Mvc;

namespace DocuAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IS3Service _s3Client;
        private readonly ILogger<TestController> _logger;

        public TestController(IS3Service s3Client, ILogger<TestController> logger)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("Testing S3 connection");
                var buckets = await _s3Client.ListBucketsAsync();
                return Ok(buckets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to S3");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}