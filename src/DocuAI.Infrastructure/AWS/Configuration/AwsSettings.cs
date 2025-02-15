namespace DocuAI.Infrastructure.AWS.Configuration
{
    /// <summary>
    /// Strongly typed AWS configuration settings.
    /// </summary>
    public class AwsSettings
    {
        public string Region { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string S3Bucket { get; set; } = string.Empty;
        public string BedrockModelId { get; set; } = string.Empty;
        public string SummarizationQueueUrl { get; set; } = string.Empty;
        public string KnowledgeBaseId { get; set; } = string.Empty;
        public string KnowledgeModelArn { get; set; } = string.Empty;


    }
}
