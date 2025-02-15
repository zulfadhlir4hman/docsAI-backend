using System.Threading.Tasks;
using DocuAI.Core.Gateway.AWS;  // Contains IDocumentService
       // Contains IBedrockService

namespace DocuAI.Core.Features.Documents.Application.Summaries
{
    /// <summary>
    /// Synchronous handler that retrieves the document text from S3 and calls the Bedrock service
    /// immediately to generate a summary.
    /// </summary>
    public class GenerateSummarySyncCommandHandler
    {
        private readonly IBedrockService _bedrockService;
        private readonly IDocumentService _documentService;

        public GenerateSummarySyncCommandHandler(IBedrockService bedrockService, IDocumentService documentService)
        {
            _bedrockService = bedrockService;
            _documentService = documentService;
        }

        public async Task<string> Handle(GenerateSummaryCommand command)
        {
            // Retrieve the document text from S3.
            var documentText = await _documentService.GetDocumentTextAsync(command.DocumentId.ToString());
            // Immediately generate and return the summary.
            return await _bedrockService.GenerateSummaryAsync(documentText);
        }
    }
}
