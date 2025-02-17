using Microsoft.AspNetCore.Mvc;

namespace DocuAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComparisonController : ControllerBase
{
    private readonly IDocumentQueueService _documentQueueService;

    public ComparisonController(IDocumentQueueService documentQueueService)
    {
        _documentQueueService = documentQueueService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateComparison([FromBody] ComparisonRequest request)
    {
        var jobId = await _documentQueueService.EnqueueJob("original/"+request.OriginalDocumentKey, "original/" + request.RevisedDocumentKey);
        return Ok(new { jobId });
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetComparisonStatus(string jobId)
    {
        var status = await _documentQueueService.GetJobStatus(jobId);
        
        if (status == null)
        {
            return NotFound();
        }

        return Ok(status);
    }
}

public class ComparisonRequest
{
    public string OriginalDocumentKey { get; set; } = string.Empty;
    public string RevisedDocumentKey { get; set; } = string.Empty;
}