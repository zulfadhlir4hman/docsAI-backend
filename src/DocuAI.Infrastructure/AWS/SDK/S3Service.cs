
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DocuAI.Infrastructure.AWS.Configuration;
using Microsoft.Extensions.Options;

namespace DocuAI.Infrastructure.AWS.SDK
{
    public interface IS3Service
    {
        Task<Stream> GetObjectStreamAsync(string key);
        Task<string> GetObjectTextAsync(string key);
        Task UploadFileAsync(string filePath, string key);
        Task<List<S3Bucket>> ListBucketsAsync();
    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettings _awsSettings;

        public S3Service(IAmazonS3 s3Client, IOptions<AwsSettings> awsSettings)
        {
            _s3Client = s3Client;
            _awsSettings = awsSettings.Value;

            _awsSettings = awsSettings.Value ?? throw new ArgumentNullException(nameof(awsSettings));

            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(_awsSettings.Region)
            };

            //_s3Client = new AmazonS3Client(credentials, config);
            var credentials = new SessionAWSCredentials(
                         _awsSettings.AccessKeyId,      // Get from settings
                        _awsSettings.SecretAccessKey,  // Get from settings
                        _awsSettings.SessionToken      // Get from settings
                    );
            _s3Client = new AmazonS3Client(credentials, config);

        }

        /// <summary>
        /// Retrieves the object stream from S3 for the configured bucket.
        /// </summary>
        public async Task<Stream> GetObjectStreamAsync(string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = _awsSettings.S3Bucket,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }

        /// <summary>
        /// Reads the S3 object's content as text.
        /// </summary>
        public async Task<string> GetObjectTextAsync(string key)
        {
            using var stream = await GetObjectStreamAsync(key);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public async Task UploadFileAsync(string filePath, string key)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _awsSettings.S3Bucket,
                    Key = key,
                    FilePath = filePath
                };

                await _s3Client.PutObjectAsync(putRequest);
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

        public async Task<List<S3Bucket>> ListBucketsAsync()
        {
            try
            {
                var response = await _s3Client.ListBucketsAsync();
                return response.Buckets;
            }
            catch (AmazonS3Exception ex)
            {
                throw;
            }
        }

        
    }
}