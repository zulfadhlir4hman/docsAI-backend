using System;

namespace DocuAI.Core.Features.Documents.Application.Summaries
{
    // Command representing a request to generate a summary
    public class GenerateSummaryCommand
    {
        public string DocumentId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
