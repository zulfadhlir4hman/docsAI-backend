using System;

namespace DocuAI.Core.Features.Documents.Domain
{
    // Minimal Document entity
    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
