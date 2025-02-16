using Microsoft.AspNetCore.Mvc;
using DocuAI.Core.Features.Documents.Application.Summaries;

namespace DocuAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummariesController : ControllerBase
    {
        private readonly GenerateSummarySyncCommandHandler _handler;

        public SummariesController(GenerateSummarySyncCommandHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSummary([FromBody] GenerateSummaryCommand command)
        {
            var summary = await _handler.Handle(command);
            return Ok(summary);
        }
    }
}
