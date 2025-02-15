using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocuAI.Core.Gateway.AWS
{
    public interface IBedrockService
    {
        Task<string> GenerateSummaryAsync(string documentText);
        Task<string> GetChatbotResponseAsync(string question);
        Task<string> ReviewDocumentDraftAsync(string draftText);
    }
}