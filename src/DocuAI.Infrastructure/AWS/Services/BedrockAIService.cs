using DocuAI.Core.Gateway.AWS;
using DocuAI.Infrastructure.AWS.SDK;


namespace DocuAI.Infrastructure.AWS.Services
{
    /// <summary>
    /// Provides high-level methods for generating document summaries, chatbot responses,
    /// and comparing document drafts with reference standards using AWS Bedrock.
    /// All Bedrock-related logic is encapsulated here.
    /// </summary>
    public class BedrockAIService : IBedrockService
    {
        private readonly AwsBedrockClient _bedrockClient;

        public BedrockAIService(AwsBedrockClient bedrockClient)
        {
            _bedrockClient = bedrockClient;
        }


        public async Task<string> GenerateSummaryAsync(string documentText)
        {
            var prompt = $"Summarize the following document concisely:\n\n{documentText}";
            return await _bedrockClient.GetLLMResponseAsync(prompt);
        }


        public async Task<string> GetChatbotResponseAsync(string question)
        {
            var prompt = $"Answer the following question:\n\n{question}";
            return await _bedrockClient.GetLLMResponseAsync(prompt);
        }


        public async Task<string> ReviewDocumentDraftAsync(string draftText)
        {
            var response = await _bedrockClient.ReviewDocumentDraftAsync(draftText);
            return response;
        }



    }
}
