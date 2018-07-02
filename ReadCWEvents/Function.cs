using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ReadCWEvents
{
    public class Function
    {
        public static readonly string TOPIC_ENV_KEY = "TOPIC_ARN";
        public async Task FunctionHandler(CWEvent<CWCustomEvent<TableDescription>> input, ILambdaContext context)
        {
            // var logs = input.Awslogs.DecodeData();
            // System.Console.WriteLine(input["id"]);
            try
            {
                System.Console.WriteLine(input.Id);
                System.Console.WriteLine(input.Detail);
                if (input.Detail != null && input.Detail.ResponseElements != null)
                {

                    var client = new AmazonSimpleNotificationServiceClient();
                    var tbl = input.Detail.ResponseElements;
                    System.Console.WriteLine(tbl);
                    var topicArn = System.Environment.GetEnvironmentVariable(TOPIC_ENV_KEY);
                    if (string.IsNullOrEmpty(topicArn))
                    {
                        throw new Exception("Couldn't find Topic ARN");
                    }
                    System.Console.WriteLine("Sending to topic...");
                    System.Console.WriteLine(JsonConvert.SerializeObject(tbl));
                    await client.PublishAsync(new PublishRequest
                    {
                        TopicArn = topicArn,
                        Message = JsonConvert.SerializeObject(tbl)
                    });
                    // System.Console.WriteLine(resp.HttpStatusCode);
                    // System.Console.WriteLine(resp.MessageId);


                    System.Console.WriteLine(input.Detail.ResponseElements);
                    System.Console.WriteLine(input.Detail.ResponseElements.TableName);
                    System.Console.WriteLine(input.Detail.ResponseElements.TableId);
                    foreach (var ks in input.Detail.ResponseElements.KeySchema)
                    {
                        System.Console.WriteLine(ks.AttributeName);
                        System.Console.WriteLine(ks.KeyType.Value);
                    }
                }
                System.Console.WriteLine(context.AwsRequestId);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Saved the environment.");
                System.Console.WriteLine(e.Message);
            }
        }
    }

}
