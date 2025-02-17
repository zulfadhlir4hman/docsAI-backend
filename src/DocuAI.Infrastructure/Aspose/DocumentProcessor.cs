using Aspose.Words;
using DocuAI.Infrastructure.AWS.SDK;
using Microsoft.Extensions.Logging;

namespace DocuAI.Infrastructure.Aspose
{
    public class DocumentProcessor : IDocumentProcessor
    {
    private readonly IS3Service _s3Service;
    private readonly string _tempPath;
    private readonly ILogger<DocumentProcessor> _logger;

        public DocumentProcessor(
        IS3Service s3Service,
        ILogger<DocumentProcessor> logger)
    {
        _s3Service = s3Service;
        _tempPath = Path.Combine(Path.GetTempPath(), "DocuAI");
        _logger = logger;
    }

        public async Task<ProcessingResult> ProcessDocuments(DocumentJob job)
        {
            var tempDir = Path.Combine(_tempPath, job.JobId);
            try 
            {
                _logger.LogInformation("Processing job {JobId}", job.JobId);
                Directory.CreateDirectory(tempDir);

                var (comparisonKey, changesKey) = await ProcessDocumentInternal(
                    tempDir, 
                    job.OriginalKey, 
                    job.RevisedKey
                );

                return new ProcessingResult(comparisonKey, changesKey, JobStatus.Completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job {JobId}", job.JobId);
                return new ProcessingResult(null, null, JobStatus.Failed);
            }
            finally
            {
                CleanupTempDirectory(tempDir);
            }
        }

        private async Task<(string comparisonKey, string changesKey)> ProcessDocumentInternal(
            string tempDir, 
            string originalKey, 
            string revisedKey)
        {
            var originalPath = Path.Combine(tempDir, "original.docx");
            var revisedPath = Path.Combine(tempDir, "revised.docx");
            var comparisonPath = Path.Combine(tempDir, "comparison.docx");
            var changesPath = Path.Combine(tempDir, "changes.docx");

            // Download files from S3
            await DownloadFileAsync(originalKey, originalPath);
            await DownloadFileAsync(revisedKey, revisedPath);

            var originalDoc = new Document(originalPath);
            var revisedDoc = new Document(revisedPath);

            // Generate comparison document with highlights
            originalDoc.Compare(revisedDoc, "Reviewer", DateTime.Now);
            originalDoc.Save(comparisonPath);

            // Extract changes to separate file
            var extractedDoc = new Document();
            var builder = new DocumentBuilder(extractedDoc);

            foreach (var revision in originalDoc.Revisions)
            {
                builder.Writeln($"Change Type: {revision.RevisionType}");
                builder.Writeln($"Content: {revision.ParentNode.GetText()}");
                if (revision.ParentNode is Paragraph para)
                {
                    builder.Writeln($"Location: Paragraph {para.Document.IndexOf(para) + 1}");
                }
                builder.Writeln("---");
            }
            extractedDoc.Save(changesPath);

            // Upload to S3
            var comparisonKey = $"comparisons/{Guid.NewGuid()}.docx";
            var changesKey = $"changes/{Guid.NewGuid()}.docx";

            await UploadFileAsync(comparisonPath, comparisonKey);
            await UploadFileAsync(changesPath, changesKey);

            return (comparisonKey, changesKey);
        }

        private async Task DownloadFileAsync(string key, string destinationPath)
        {
            using var stream = await _s3Service.GetObjectStreamAsync(key);
            using var fileStream = File.Create(destinationPath);
            await stream.CopyToAsync(fileStream);
        }

        private async Task UploadFileAsync(string filePath, string key)
        {
            await _s3Service.UploadFileAsync(filePath, key);
        }

        private void CleanupTempDirectory(string tempDir)
        {
            try
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup directory {Dir}", tempDir);
            }
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_tempPath))
                    Directory.Delete(_tempPath, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing DocumentProcessor");
            }
        }
    }
}