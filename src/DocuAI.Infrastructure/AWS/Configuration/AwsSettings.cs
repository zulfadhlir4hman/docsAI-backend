namespace DocuAI.Infrastructure.AWS.Configuration
{
    /// <summary>
    /// Strongly typed AWS configuration settings.
    /// </summary>
    public class AwsSettings
    {
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string S3Bucket { get; set; } = string.Empty;
        public string BedrockModelId { get; set; } = string.Empty;
        public string KnowledgeBaseId { get; set; } = string.Empty;
        public string KnowledgeModelArn { get; set; } = string.Empty;


    }
}
