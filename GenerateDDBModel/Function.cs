using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using GenerateDDBModel.Generator;
using static GenerateDDBModel.Generator.Generator;
using Newtonsoft.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GenerateDDBModel
{
    public class Function
    {
        public static readonly string BUCKET_NAME_KEY = "BUCKET_NAME";
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
        {
            foreach (var record in snsEvent.Records)
            {
                var snsRecord = record.Sns;
                Console.WriteLine($"[{record.EventSource} {snsRecord.Timestamp}] Message = {snsRecord.Message}");
                if (snsRecord.Message != null && !string.IsNullOrEmpty(snsRecord.Message))
                {
                    var serialize = new JsonSerializer();
                    using (var reader = new JsonTextReader(new StringReader(snsRecord.Message)))
                    {
                        var ddbTable = serialize.Deserialize<TableDescription>(reader);
                        var code = FromDDBTableModel(ddbTable);
                        var bucketName = System.Environment.GetEnvironmentVariable(BUCKET_NAME_KEY);
                        if(string.IsNullOrEmpty(bucketName))
                        {
                            throw new Exception("Couldn't find S3 bucket");
                        }
                        var client = new AmazonS3Client();
                        var response = await client.PutObjectAsync(new PutObjectRequest 
                        {
                            BucketName = bucketName,
                            ContentBody = code,
                            Key = $"{GetClassName(ddbTable)}.cs"
                        });
                        System.Console.WriteLine(response.HttpStatusCode);
                        System.Console.WriteLine(response.ETag);
                        System.Console.WriteLine(response.VersionId);
                    }
                }
            }
        }
    }
}
