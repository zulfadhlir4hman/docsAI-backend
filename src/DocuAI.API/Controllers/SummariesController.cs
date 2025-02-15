using Microsoft.AspNetCore.Mvc;
using DocuAI.Core.Features.Documents.Application.Summaries;

namespace DocuAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SummariesController : ControllerBase
    {
        private readonly GenerateSummaryCommandHandler _handler;

        // The handler is injected via DI
        public SummariesController(GenerateSummaryCommandHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("generate")]
        public IActionResult GenerateSummary([FromBody] GenerateSummaryCommand command)
        {
            var summary = _handler.Handle(command);
            return Ok(summary);
        }
    }
}
