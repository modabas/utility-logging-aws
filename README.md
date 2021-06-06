# .NET Core ILogger implemetation for AWS CloudWatch Logs

This repository contains ILogger and ILoggerProvider implementation of Microsoft.Extensions.Logging for AWS CloudWatch Logs that supports scopes and structured logging.
By default, log format is Json, however it's possible to create other formats with custom ILogRenderer implementations for AwsLoggerProvider.
The logs can be viewed and searched using the [AWS CloudWatch Console](https://console.aws.amazon.com/cloudwatch/).
CloudWatch Agent requires necessary policies attached to IAM Role. For more information, check [Create IAM Roles and Users for Use with the CloudWatch Agent](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/create-iam-roles-for-cloudwatch-agent.html).

This project started off by being a slightly modified version of [bartpio/aws-logging-dotnet-structured](https://github.com/bartpio/aws-logging-dotnet-structured),
then morphed into an ILogger implementation, that directly uses AWSSDK.CloudwatchLogs, by importing and modifying code from 2 other projects:
-A standart ILogger and ILoggerProvider implementation from [dotnet/runtime](https://github.com/dotnet/runtime/tree/master/src/libraries) like Console logger or EventLog logger,
-AWSSDK.CloudWatchLogs integration from [aws/aws-logging-dotnet](https://github.com/aws/aws-logging-dotnet).


## Configuration

The configuration is setup in the configuraton json file.
Use `AWS` section under `Logging` block to configure AwsLogger and underlying AmazonCloudWatchLogsClient behaviour.
`AWS` section must contain a `LogGroup` key and corresponding value.
For information on LogLevel configuration, check [Logging in .NET Core and ASP.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1).

```json
"Logging": {
  "AWS": {
    "Region": "us-east-1",
    "LogGroup": "AspNetCore.WebSample",
    "IncludeSemantics": true,
    "IncludeScopes": true,
    "IncludeException": true,
    "IncludeLogLevel": true,
    "IncludeCategory": true,
    "IncludeNewline": false,
    "IncludeEventId": true,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
```

Within .NET Core application, add a `ConfigureLogging()` method call to existing Generic Host Builder (Microsoft.Extensions.Hosting) chain for injecting AwsLoggerProvider and AwsLogger.

```csharp
        hostBuilder
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", false, true);
        })
        .ConfigureLogging((hostingContext, logging) =>
        {
            logging.ClearProviders();
            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            logging.AddAwsLogging(hostingContext.Configuration.GetAwsLoggingConfigSection());
			//add other log providers
        })
		//more configuration
```

If IncludeSemantics is set to true, parameter values for structured logging will be logged under semantics section in log json output.
For example, for code below, values for id and DateTime.Now will be logged under semantics.Id and semantics.RequestTime respectively.

```csharp
    _logger.LogInformation("Getting item {Id} at {RequestTime}", id, DateTime.Now);
```

If IncludeScopes is set to true, parameters passed to ILogger.BeginScope will be logged with each Log under scope section in log json output.

If IncludeException is set to true, exception message and full exception object string will be included for any exception passed to ILogger. Data will be under exceptionMessage and exception fields in log json output.
For default renderer, exception will be serialized to json by using ExceptionStringConverter.cs in Converters folder.

If IncludeCategory is set to true, type name of T for ILogger<T> object will be logged under categoryName field in log json output.

If IncludeLogLevel is set to true, log severity ("Information", "Error", "Debug", ...) will be logged under logLevel field in log json output.

Json object serialization behaviour can be changed by passing a JsonRenderer instance intiated with desired JsonSerializer options into AddAwsLogging() method during dependency injection.
Log output format (eg: xml or plain text instead of json) can be changed by implementing a custom ILogRenderer and passing in into AddAwsLogging() method during dependency injection.


## Sample Output

With default ILogRenderer (json) and Scope, Semantics, Category, LogLevel keys enabled (set to true in configuration file); a sample output looks like:

```json
{
  "logLevel": "Information",
  "categoryName": "Utility.Middlewares.TracingMiddleware",
  "message": "RequestPath= (/), Duration= (12) ms, ResponseStatusCode= (200), HttpMethod= (GET), BrowserType= (Desktop)",
  "semantics": {
    "requestPath": "/",
    "duration": 12,
    "responseStatusCode": 200,
    "httpMethod": "GET",
    "browserType": "Desktop",
    "{OriginalFormat}": "RequestPath= ({requestPath}), Duration= ({duration}) ms, ResponseStatusCode= ({responseStatusCode}), HttpMethod= ({httpMethod}), BrowserType= ({browserType})"
  },
  "scope": {
    "SpanId": "61d4eceef58a174d",
    "TraceId": "0fc5c5fca4b6674183d3c9eff8b659e2",
    "ParentId": "0000000000000000",
    "ConnectionId": "0HM984L85N5TL",
    "RequestId": "0HM984L85N5TL:00000001",
    "RequestPath": "/",
    "interchangeId": "743a2726-e130-4802-9ffb-0b46cc80f2d5",
    "parentInterchangeId": "",
    "{OriginalFormat}": "InterchangeId:({interchangeId}), ParentInterchangeId:({parentInterchangeId})"
  }
}
```