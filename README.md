# C# DyanmoDB Object Persistence Model Generator
Using [Roslyn](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/), the open source .NET compiler, this project automates the creation of boilerplate C# code for using the [.NET Object Persistence Model with DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DotNetSDKHighLevel.html). 

The **.NET Object Persistence Model for DynamoDB** is a high level API that maps your DynamoDB tables to classes in C#, operating very similarly to an [ORM tool(Object-relational mapper)](https://en.wikipedia.org/wiki/Object-relational_mapping).

The automation tool in this repo uses [CloudWatch](https://aws.amazon.com/cloudwatch/) to listen for when new DynamoDB tables are created within your AWS account.

When a new table is created, the automation tool generates a `partial` class in C# with automatically generated annotations for a DynamoDB table name, partition key, and sort key if present. The class is deposited in an [S3 Bucket](https://aws.amazon.com/s3/) where you can download the `.cs` file to a .NET project of your choosing.
![C# Generated Code](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/DDB-OPM-CS-Code.png)
# Installation
Using [CloudFormation](https://aws.amazon.com/cloudformation/), you can automatically create the infrastructure resources and deploy the code for this automation tool without the need to write code or manage resources yourself. 

The assets are publicly available at the following URLs:
* [https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/GenerateDDBModel.zip](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/GenerateDDBModel.zip) - A `.zip` package containing the compiled C# code that generates the DynamoDB Object Persistence Model classes.
* [https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/ReadCWEvents.zip](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/ReadCWEvents.zip) - A `.zip` package containing the compiled C# code that processes events from the CloudWatch Rule created by this CloudFormation stack.
* [https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/template.json](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/template.json) - A CloudFormation template to generate all needed resources to run this automation tool.

You won't need to access any of these files directly, but they are available to download if you would like to inspect the contents. You will only work with [CloudFormation](https://aws.amazon.com/cloudformation/) to install this automation tool.
## Steps:
Navigate to [CloudFormation](https://console.aws.amazon.com/cloudformation/home) in the AWS Console and follow these instructions:
1. Click `Create Stack`.
![Create Stack](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/CF-Create-Stack.png)
2. Provide `https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/template.json` as the URL to the CloudFormation template in S3.
![CF Template in S3](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/Enter-CF-Template-Link.png)
    * You can optionally view the the template in the Designer.
    ![CF Designer](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/Optional-Check-CF-Template-Design.png)
3. Enter a descriptive CloudFormation stack name.
![CF Stack Name](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/CF-Stack-Name.png)
4. Leave the defaults and click `Next`.
![CF Defaults](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/CF-Leave-Defaults-Next.png)
5. Since this template uses the `transform` CloudFormation feature, you need to create a change set before you can execute the template.
![CF Change Set](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/CF-Create-Change-Set.png)
    * See [AWS SAM](https://github.com/awslabs/serverless-application-model) and [CloudFormation transform](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/transform-aws-serverless.html) for more details.
6. Once the change set completes, you can then `Execute` the CloudFormation template.
![CF Execute](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/CF-Change-Set-Created-Execute.png)
7. You'll know the CloudFormation stack has been created in your AWS account when the stack status is `CREATE_COMPLETE`.
![CF Created](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/CF-Create-Complete.png) 
# Usage
The automation tool will generate an annotated C# class anytime you create a new DynamoDB table, regardless of how that table is created. You can create a new DynamoDB table through the [AWS APIs](https://docs.aws.amazon.com/amazondynamodb/latest/APIReference/API_CreateTable.html), the [AWS CLI](https://docs.aws.amazon.com/cli/latest/reference/dynamodb/create-table.html), or through the [AWS Console](https://console.aws.amazon.com/dynamodb/home). We'll review how to create a table through the AWS Console.
## Steps
Navigate to [DynamoDB in the AWS Console](https://console.aws.amazon.com/dynamodb/home).
1. Click `Create Table`.
![DDB Create Table](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/DDB-Create-Table.png)
2. Enter information about your table. We'll create an example table for a blog, with the table name `AMSXBGBlogTable`, a partition key named `PostId` of type `String`, and a sort key named `PublishedOn` of type `Number`.
    * For this table, I added a prefix of a company name to the table name. Adding prefixes like company, department, etc. help to identify the table later.
    * We'll see later on that the automation tool picks a C# type of `decimal` for numbers in a DynamoDB table. We'll review some choices you have if you don't want to use `decimal` as the C# type in the generated code.
    * For more information on DynamoDB partition keys and sort keys, [see this DynamoDB Best Practices guide](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/best-practices.html). 
![DDB Table Info](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/DDB-Table-Info.png)
3. The automation tool will run without any other prompting needed. You can check for an [S3 Bucket](https://aws.amazon.com/s3/) that follows the naming convention of `ddb-model-code-bucket-%your_aws_account_id%`.
![S3 DDB Code Bucket ](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/S3-Bucket-DDB-Code.png)
4. You should find a file in this [S3 Bucket](https://aws.amazon.com/s3/) with the same name as your DynamoDB table. The pattern for the file name is `%ddb_table_name%DDBModel.cs`.
![S3 DDB Code Bucket File](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/S3-Bucket-Auto-Generated-CS-Code.png)
5. Since S3 recognizes this as a text file, when you click download the file will save to your computer as a `.txt` file. You can simply rename the file when it downloads to a `.cs` file, or you can use the `Download As` feature of S3.
![S3 Download As](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/S3-Bucket-Download-As-Step-1.png)
6. Right click the link and choose `Save Link As`.
![S3 Download As Save](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/S3-Bucket-Download-As-Step-2.png)
![S3 Download As Save Link As](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/S3-Bucket-Download-As-Step-3.png)
7. Add `.cs` to the file name before saving to your computer.
![S3 File Dialog](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/S3-Bucket-Download-As-Step-4.png)

You should have a `partial` C# class with complete annotations for the DynamoDB table you created. Since the class is `partial`, you can create another file in your .NET project with the same name and add more properties that couldn't be inferred from your DynamoDB table. 

When you want to use this class in a .NET project, be sure to include the [`AWSSDK.DynamoDBv2` package from Nuget](https://www.nuget.org/packages/AWSSDK.DynamoDBv2/) in your project dependencies. If it is included in your project, you should see a reference your `.csproj` file:
```xml
<PackageReference Include="AWSSDK.DynamoDBv2" Version="3.3.12" />
```

![Generated Code](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDBSteps/DDB-OPM-CS-Code.png)
## Considerations for the Tool
If you want different generated types, such as an `int` type in the C# class when a DynamoDB key uses type `Number`, you can simply change the type in the generated file, or you can deploy your own custom version of the tool by altering the code in this repo and uploading the packages to your own S3 Buckets. You'll only need to change the `CodeUri` fields in the `template.json` CloudFormation template included in this repo.
# Uninstall 
Removing this automation tool is as easy as deleting the CloudFormation stack created in your AWS account. For more information on deleting a CloudFormation stack, [see this documentation](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/cfn-console-delete-stack.html).

You may encounter an error when trying to delete the CloudFormation stack. If your generated S3 Bucket contains files when you attempt a stack deletion, CloudFormation will produce an error. You'll need to empty the S3 Bucket before you're allowed to delete the CloudFormation stack. [See this documentation for how to empty an S3 Bucket](https://docs.aws.amazon.com/AmazonS3/latest/user-guide/empty-bucket.html).
# Architecture Review and Inspiration
Writing an automation tool like this is relatively easy and doesn't take a lot of time. Since the resources used are all serverless, you only pay for time that the automation tool executes and you don't need to provision or manage any servers for this tool to function. I wanted to include a diagram of the pieces that make this tool work, so that you may be inspired to build your own automation tools that use the power of Roslyn and serverless architecture.
![Architecture Diagram](https://s3-us-west-2.amazonaws.com/amsxbg-ddb-code-generator/DDB-Object-Persistence-Model-Generator-Lucid.svg)
# Additional C# Tools for AWS
If you're a C# developer on AWS be sure to check out these tools that will also increase your productivity.
* [Visual Studio Toolkit](https://aws.amazon.com/visualstudio/)
* .NET CLI AWS Extension
    * [AWS Lambda Extensions Documentation](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-coreclr-deployment-package.html)
    * [GitHub repo](https://github.com/aws/aws-extensions-for-dotnet-cli)
* [AWS Tools for PowerShell](https://aws.amazon.com/powershell/)