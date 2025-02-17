namespace DocuAI.Infrastructure.Aspose
{
    public interface IDocumentProcessor
    {
        Task<ProcessingResult> ProcessDocuments(DocumentJob job);
    }

    public record ProcessingResult(
        string ComparionFileKey,
        string ExtractedChangesKey,
        JobStatus Status
    );

    public record DocumentJob(
        string JobId,
        string OriginalKey,
        string RevisedKey
    );

    public enum JobStatus
    {
        Queued,
        Processing,
        Completed,
        Failed
    }
}