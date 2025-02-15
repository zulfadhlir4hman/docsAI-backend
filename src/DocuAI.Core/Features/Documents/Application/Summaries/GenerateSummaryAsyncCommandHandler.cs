using System.Threading.Tasks;
using DocuAI.Core.Gateway.AWS;  // Contains ISummarizationJobQueue and GenerateSummaryCommand

namespace DocuAI.Core.Features.Documents.Application.Summaries
{
    /// <summary>
    /// Asynchronous handler that enqueues a summarization job for later processing.
    /// </summary>
    public class GenerateSummaryAsyncCommandHandler
    {
        private readonly ISummarizationJobQueue _jobQueue;

        public GenerateSummaryAsyncCommandHandler(ISummarizationJobQueue jobQueue)
        {
            _jobQueue = jobQueue;
        }

        public async Task<string> Handle(GenerateSummaryCommand command)
        {
            // Enqueue the summarization job and return a job ID.
            return await _jobQueue.EnqueueSummaryJobAsync(command);
        }
    }
}
