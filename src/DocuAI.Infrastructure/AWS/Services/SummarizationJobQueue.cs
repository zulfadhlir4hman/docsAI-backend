using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using DocuAI.Core.Features.Documents.Application.Summaries;
using DocuAI.Core.Gateway.AWS;
using DocuAI.Infrastructure.AWS.Configuration;
using Microsoft.Extensions.Options;

namespace DocuAI.Infrastructure.AWS.Services
{
    /// <summary>
    /// Implements the job queue using Amazon SQS.
    /// </summary>
    public class SummarizationJobQueue : ISummarizationJobQueue
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly AwsSettings _awsSettings;

        public SummarizationJobQueue(IAmazonSQS sqsClient, IOptions<AwsSettings> awsSettings)
        {
            _sqsClient = sqsClient;
            _awsSettings = awsSettings.Value;
        }

        public async Task<string> EnqueueSummaryJobAsync(GenerateSummaryCommand command)
        {
            // Serialize the command to JSON.
            var payload = JsonSerializer.Serialize(command);
            var sendRequest = new SendMessageRequest
            {
                QueueUrl = "sqs url", // Ensure you add this property in AwsSettings.
                MessageBody = payload
            };

            var response = await _sqsClient.SendMessageAsync(sendRequest);
            return response.MessageId;
        }
    }
}
