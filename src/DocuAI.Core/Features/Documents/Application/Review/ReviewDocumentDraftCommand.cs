using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocuAI.Core.Features.Documents.Application.Review
{
    public class ReviewDocumentDraftCommand
    {
        public string DraftText { get; set; } = string.Empty;
    }
}