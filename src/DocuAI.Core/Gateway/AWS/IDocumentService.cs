using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocuAI.Core.Gateway.AWS
{
    public interface IDocumentService
    {
        /// <summary>
        /// Retrieves a document stream given an S3 object key.
        /// </summary>
        Task<Stream> GetDocumentStreamAsync(string key);

        /// <summary>
        /// Retrieves the document text given an S3 object key.
        /// </summary>
        Task<string> GetDocumentTextAsync(string key);
    }
}