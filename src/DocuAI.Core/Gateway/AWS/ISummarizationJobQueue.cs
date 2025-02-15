using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocuAI.Core.Features.Documents.Application.Summaries;

namespace DocuAI.Core.Gateway.AWS
{
    public interface ISummarizationJobQueue
    {
        /// <summary>
        /// Enqueues a summarization command and returns a job ID.
        /// </summary>
        Task<string> EnqueueSummaryJobAsync(GenerateSummaryCommand command);
    }
}