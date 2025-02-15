using Amazon.S3;
using Amazon.SQS;
using Amazon.BedrockRuntime;
using Amazon.Extensions.NETCore.Setup;
using DocuAI.Core.Gateway.AWS;
using DocuAI.Infrastructure.AWS.Configuration;
using DocuAI.Infrastructure.AWS.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DocuAI.Infrastructure.AWS.SDK;

namespace DocuAI.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind strongly typed AWS settings using a lambda.
            services.Configure<AwsSettings>(options => configuration.GetSection("AWS").Bind(options));

            // Configure AWS options.
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());

            // Register AWS S3 and SQS clients.
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonSQS>();

            // Register the concrete Bedrock client.
            services.AddAWSService<AmazonBedrockRuntimeClient>();

            // Register the new S3 service.
            services.AddScoped<IS3Service, S3Service>();

            // Register the document service that uses the S3 service.
            services.AddScoped<IDocumentService, S3DocumentService>();

            // Register the summarization job queue.
            services.AddScoped<ISummarizationJobQueue, SummarizationJobQueue>();

            // Register other AWS Bedrock related services.
            services.AddScoped<AwsBedrockClient>();
            services.AddScoped<BedrockAIService>();

            // Optionally, register other services if needed.
            // services.AddSingleton<AsposeComparisonService>();

            return services;
        }
    }
}
