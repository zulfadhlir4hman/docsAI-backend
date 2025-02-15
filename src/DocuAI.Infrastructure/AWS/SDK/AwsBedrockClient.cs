using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using DocuAI.Infrastructure.AWS.Configuration;
using Microsoft.Extensions.Options;

namespace DocuAI.Infrastructure.AWS.SDK
{
    /// <summary>
    /// AWS Bedrock client that wraps AWS SDK calls to invoke a foundation model.
    /// </summary>
    public class AwsBedrockClient
    {
        private readonly AmazonBedrockRuntimeClient _bedrockClient;
        private readonly AwsSettings _awsSettings;

        public AwsBedrockClient(AmazonBedrockRuntimeClient bedrockClient, IOptions<AwsSettings> awsSettings)
        {
            _bedrockClient = bedrockClient;
            _awsSettings = awsSettings.Value;
        }

        /// <summary>
        /// Sends a simple prompt to AWS Bedrock and returns the generated LLM response.
        /// </summary>
        public async Task<string> GetLLMResponseAsync(string prompt)
        {
            // Build JSON payload with prompt and model parameters.
            var payload = JsonSerializer.Serialize(new
            {
                prompt = prompt,
                maxTokens = 100,
                temperature = 0.7
            });

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            using var payloadStream = new MemoryStream(payloadBytes);

            // Construct the request using your LLM model ID from settings.
            var request = new InvokeModelRequest
            {
                ModelId = _awsSettings.BedrockModelId,
                Body = payloadStream,
                ContentType = "application/json"
            };

            var response = await _bedrockClient.InvokeModelAsync(request);
            using var reader = new StreamReader(response.Body);
            var responseBody = await reader.ReadToEndAsync();
            return responseBody;
        }

        
        /// <summary>
        /// Sends a conversation request to AWS Bedrock using a list of messages and returns the combined response.
        /// This method is useful for comparing a document draft with a reference standard or for conversational navigation.
        /// </summary>
        /// <param name="messages">The conversation messages to send.</param>
        /// <returns>A task that returns the combined response text.</returns>
        public async Task<string> ReviewDocumentDraftAsync(string draftText)
        {
            // Build a query text using the document draft.
            string queryText = $"Review the following document draft and evaluate its compliance with the reference standard: {draftText}";

            // Create a new AmazonBedrockAgentRuntimeClient using the configured region.
            using var agentClient = new AmazonBedrockAgentRuntimeClient(RegionEndpoint.GetBySystemName(_awsSettings.Region));

            var request = new RetrieveAndGenerateRequest
            {
                Input = new RetrieveAndGenerateInput { Text = queryText },
                RetrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration
                {
                    KnowledgeBaseConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration
                    {
                        KnowledgeBaseId = _awsSettings.KnowledgeBaseId,   // Reference standard's knowledge base ID.
                        ModelArn = _awsSettings.KnowledgeModelArn         // Model ARN configured for knowledge queries.
                    },
                    Type = RetrieveAndGenerateType.KNOWLEDGE_BASE
                }
            };

            try
            {
                var response = await agentClient.RetrieveAndGenerateAsync(request);

                // Example: Extract context and document URL from the first citation.
                if (response.Citations != null && response.Citations.Count > 0 &&
                    response.Citations[0].RetrievedReferences != null &&
                    response.Citations[0].RetrievedReferences.Count > 0)
                {
                    var reference = response.Citations[0].RetrievedReferences[0];
                    string context = reference.Content?.Text ?? string.Empty;
                    string docUrl = reference.Location?.S3Location?.Uri ?? "No URL available";
                    return $"Compliance Analysis:\n{context}\nSource Document: {docUrl}";
                }
                else
                {
                    return "No context available from the knowledge base.";
                }
            }
            catch (AmazonBedrockAgentRuntimeException ex)
            {
                return $"AWS Error: {ex.Message}";
            }
            catch (System.Exception ex)
            {
                return $"General Error: {ex.Message}";
            }
        }
    }
}
