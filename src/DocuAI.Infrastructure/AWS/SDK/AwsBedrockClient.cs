using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using DocuAI.Infrastructure.AWS.Configuration;
using Microsoft.Extensions.Options;
using Amazon.Runtime;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace DocuAI.Infrastructure.AWS.SDK
{
    /// <summary>
    /// AWS Bedrock client that wraps AWS SDK calls to invoke a foundation model.
    /// </summary>
    public class AwsBedrockClient
    {
        private readonly AmazonBedrockRuntimeClient _bedrockClient;
        private readonly AwsSettings _awsSettings;
        private readonly AWSCredentials _awsCredentials;  // Store resolved credentials

        public AwsBedrockClient(IOptions<AwsSettings> awsSettings, ILogger<AwsBedrockClient> logger)
        {
            _awsSettings = awsSettings.Value ?? throw new ArgumentNullException(nameof(awsSettings));

            try
            {
                // Decide which credentials to use.
                _awsCredentials = GetAwsCredentials(logger);

                // Create STS client to verify credentials
                var stsConfig = new AmazonSecurityTokenServiceConfig
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(_awsSettings.Region)
                };

                var stsClient = new AmazonSecurityTokenServiceClient(_awsCredentials, stsConfig);

                // Verify credentials by making a call
                var request = new GetCallerIdentityRequest();
                var callerIdentity = stsClient.GetCallerIdentityAsync(request).GetAwaiter().GetResult();
                logger.LogInformation($"Successfully authenticated as: {callerIdentity.Arn}");

                // Create Bedrock client using the same credentials
                var bedrockConfig = new AmazonBedrockRuntimeConfig
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(_awsSettings.Region)
                };

                _bedrockClient = new AmazonBedrockRuntimeClient(_awsCredentials, bedrockConfig);
            }
            catch (AmazonSecurityTokenServiceException stsEx)
            {
                logger.LogError($"STS Error: {stsEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error initializing AWS client: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Determines which AWS credentials to use based on the provided settings.
        /// Falls back to the default credentials chain if settings are missing or incomplete.
        /// </summary>
        private AWSCredentials GetAwsCredentials(ILogger logger)
        {
            bool hasAccessKey = !string.IsNullOrWhiteSpace(_awsSettings.AccessKeyId);
            bool hasSecretKey = !string.IsNullOrWhiteSpace(_awsSettings.SecretAccessKey);
            bool hasSessionToken = !string.IsNullOrWhiteSpace(_awsSettings.SessionToken);

            if (hasAccessKey && hasSecretKey)
            {
                if (hasSessionToken)
                {
                    logger.LogInformation("Using explicit temporary credentials from settings.");
                    return new SessionAWSCredentials(
                        _awsSettings.AccessKeyId,
                        _awsSettings.SecretAccessKey,
                        _awsSettings.SessionToken
                    );
                }
                else
                {
                    logger.LogWarning("Explicit session token missing. Using explicit basic credentials from settings.");
                    return new BasicAWSCredentials(
                        _awsSettings.AccessKeyId,
                        _awsSettings.SecretAccessKey
                    );
                }
            }
            else
            {
                logger.LogInformation("No explicit credentials found in settings. Using default credentials chain.");
                return FallbackCredentialsFactory.GetCredentials();
            }
        }

        /// <summary>
        /// Sends a simple prompt to AWS Bedrock and returns the generated LLM response.
        /// </summary>
        public async Task<string> GetLLMResponseAsync(string prompt)
        {
            // Build JSON payload with prompt and model parameters
            var payload = JsonSerializer.Serialize(new
            {
                anthropic_version = "bedrock-2023-05-31",
                max_tokens = 100,
                temperature = 0.7,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            });

            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            using var payloadStream = new MemoryStream(payloadBytes);

            // Construct the request using your LLM model ID from settings
            var request = new InvokeModelRequest
            {
                ModelId = _awsSettings.BedrockModelId,
                Body = payloadStream,
                ContentType = "application/json"
            };

            var response = await _bedrockClient.InvokeModelAsync(request);

            // Parse the response to get the actual content
            using var streamReader = new StreamReader(response.Body);
            var jsonResponse = await streamReader.ReadToEndAsync();
            var responseNode = JsonNode.Parse(jsonResponse);

            // Extract the text from the response structure
            var responseText = responseNode?["content"]?[0]?["text"]?.GetValue<string>() ?? "";
            return responseText;
        }

        /// <summary>
        /// Sends a conversation request to AWS Bedrock using a list of messages and returns the combined response.
        /// This method is useful for comparing a document draft with a reference standard or for conversational navigation.
        /// </summary>
        /// <param name="draftText">The document draft text to review.</param>
        /// <returns>A task that returns the combined response text.</returns>
        public async Task<string> ReviewDocumentDraftAsync(string draftText)
        {
            // Build a query text using the document draft.
            string queryText = $"Review the following document draft and evaluate its compliance with the reference standard: {draftText}";

            // Create a new AmazonBedrockAgentRuntimeClient using the configured region and the same credentials.
            using var agentClient = new AmazonBedrockAgentRuntimeClient(_awsCredentials, RegionEndpoint.GetBySystemName(_awsSettings.Region));

            var request = new RetrieveAndGenerateRequest
            {
                Input = new RetrieveAndGenerateInput { Text = queryText },
                RetrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration
                {
                    KnowledgeBaseConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration
                    {
                        KnowledgeBaseId = _awsSettings.KnowledgeBaseId,   // Reference standard's knowledge base ID.
                        ModelArn = _awsSettings.KnowledgeModelArn          // Model ARN configured for knowledge queries.
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
            catch (Exception ex)
            {
                return $"General Error: {ex.Message}";
            }
        }
    }
}
