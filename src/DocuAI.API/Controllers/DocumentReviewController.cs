using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocuAI.Core.Features.Documents.Application.Review;
using DocuAI.Core.Features.Documents.Application.Summaries;
using Microsoft.AspNetCore.Mvc;

namespace DocuAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentReviewController : ControllerBase
    {
        private readonly ReviewDocumentDraftCommandHandler _reviewHandler;

        public DocumentReviewController(ReviewDocumentDraftCommandHandler reviewHandler)
        {
            _reviewHandler = reviewHandler;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> ReviewDocument([FromBody] ReviewDocumentDraftCommand command)
        {
            if (string.IsNullOrEmpty(command.DraftText))
            {
                return BadRequest("Draft text is required");
            }

            try
            {
                var result = await _reviewHandler.Handle(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(500, "An error occurred while processing the document review");
            }
        }
    }
}