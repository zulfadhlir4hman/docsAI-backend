using System.Collections.Concurrent;
using DocuAI.Infrastructure.Aspose;
using static DocumentQueueService;

public interface IDocumentQueueService
{
    Task<string> EnqueueJob(string originalKey, string revisedKey);
    Task<DocumentComparisonJob?> GetJobStatus(string jobId);
}

public class DocumentQueueService : IDocumentQueueService
{
    private readonly ConcurrentQueue<DocumentComparisonJob> _jobQueue = new();
    private readonly ConcurrentDictionary<string, DocumentComparisonJob> _jobStatus = new();
    private readonly IDocumentProcessor _documentProcessor;

    public DocumentQueueService(IDocumentProcessor documentProcessor)
    {
        _documentProcessor = documentProcessor;
    }
    
    public async Task<string> EnqueueJob(string originalKey, string revisedKey)
    {
        var job = new DocumentComparisonJob
        {
            JobId = Guid.NewGuid().ToString(),
            OriginalDocumentKey = originalKey,
            RevisedDocumentKey = revisedKey,
            Status = JobStatus.Queued,
            CreatedAt = DateTime.UtcNow
        };
        
        _jobQueue.Enqueue(job);
        _jobStatus[job.JobId] = job;
        
        // Start processing in background
        _ = ProcessQueueAsync();
        
        return job.JobId;
    }
    
    private async Task ProcessQueueAsync()
    {
        while (_jobQueue.TryDequeue(out var job))
        {
            try
            {
                job.Status = JobStatus.Processing;
                _jobStatus[job.JobId] = job;
                
                var documentJob = new DocumentJob(
                    job.JobId,
                    job.OriginalDocumentKey,
                    job.RevisedDocumentKey
                );

                var result = await _documentProcessor.ProcessDocuments(documentJob);
                
                job.Result = result;
                job.Status = result.Status;
            }
            catch
            {
                job.Status = JobStatus.Failed;
            }
            
            _jobStatus[job.JobId] = job;
        }
    }
    
    public Task<DocumentComparisonJob?> GetJobStatus(string jobId)
    {
        return Task.FromResult(_jobStatus.GetValueOrDefault(jobId));
    }

    public class DocumentComparisonJob
    {
        public string JobId { get; set; } = string.Empty;
        public string OriginalDocumentKey { get; set; } = string.Empty;
        public string RevisedDocumentKey { get; set; } = string.Empty;
        public JobStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public ProcessingResult? Result { get; set; }
    }
}