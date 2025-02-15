using DocuAI.Core.Gateway.AWS;  // IDocumentService is defined here.
using DocuAI.Infrastructure.AWS.SDK;

namespace DocuAI.Infrastructure.AWS.Services
{
    /// <summary>
    /// Service for retrieving documents from S3 and extracting their text.
    /// </summary>
    public class S3DocumentService : IDocumentService
    {
        private readonly IS3Service _s3Service;

        public S3DocumentService(IS3Service s3Service)
        {
            _s3Service = s3Service;
        }

        public async Task<Stream> GetDocumentStreamAsync(string key)
        {
            return await _s3Service.GetObjectStreamAsync(key);
        }

        public async Task<string> GetDocumentTextAsync(string key)
        {
            return await _s3Service.GetObjectTextAsync(key);
        }
    }
}
