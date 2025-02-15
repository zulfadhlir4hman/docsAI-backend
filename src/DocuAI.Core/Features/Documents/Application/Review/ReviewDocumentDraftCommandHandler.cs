using System.Threading.Tasks;
using DocuAI.Core.Features.Documents.Application.Review;
using DocuAI.Core.Gateway.AWS;

namespace DocuAI.Core.Features.Documents.Application.Summaries
{
    /// <summary>
    /// Handles the ReviewDocumentDraftCommand by passing the draft document to the Bedrock service
    /// for compliance analysis against the reference standard in the knowledge base.
    /// </summary>
    public class ReviewDocumentDraftCommandHandler
    {
        private readonly IBedrockService _bedrockService;

        public ReviewDocumentDraftCommandHandler(IBedrockService bedrockService)
        {
            _bedrockService = bedrockService;
        }

        public async Task<string> Handle(ReviewDocumentDraftCommand command)
        {
            return await _bedrockService.ReviewDocumentDraftAsync(command.DraftText);
        }
    }
}
