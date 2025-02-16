using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SQS;
using DocuAI.Core.Gateway.AWS;
using DocuAI.Infrastructure.AWS.Configuration;
using DocuAI.Infrastructure.AWS.SDK;
using DocuAI.Infrastructure.AWS.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. First, validate AWS configuration
        var awsSection = configuration.GetSection("AWS");
        if (!awsSection.Exists())
        {
            throw new InvalidOperationException("AWS configuration section is missing");
        }

        // 2. Configure AWS settings
        services.Configure<AwsSettings>(options => configuration.GetSection("AWS").Bind(options));

        // 3. Configure AWS options with explicit region
        var awsOptions = new AWSOptions
        {
            Region = RegionEndpoint.GetBySystemName(awsSection["Region"] ?? "ap-southeast-1"),
            Credentials = new BasicAWSCredentials("ASIA4MI2JPRI6F2PSCVD", " NLaIGa84CToKwMMxEfZvj/oZARNujY4EtuPaDGxd"),
        };




        services.AddDefaultAWSOptions(awsOptions);

        // 4. Register AWS service clients
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonSQS>();

        // 5. Register Bedrock client with explicit configuration
        services.AddScoped<AmazonBedrockRuntimeClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AwsSettings>>().Value;
            return new AmazonBedrockRuntimeClient(
                RegionEndpoint.GetBySystemName(options.Region));
        });

        // 6. Register application services
        services.AddScoped<IS3Service, S3Service>();
        services.AddScoped<IDocumentService, S3DocumentService>();
        services.AddScoped<ISummarizationJobQueue, SummarizationJobQueue>();
        services.AddScoped<AwsBedrockClient>();
        services.AddScoped<IBedrockService, BedrockAIService>();

        return services;
    }
}