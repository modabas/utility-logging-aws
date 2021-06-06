using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AWSLoggerTest.Classes
{
    public class Task2
    {
        private readonly ILogger<Task2> logger;

        public Task2(ILogger<Task2> logger, Task1 task1)
        {
            this.logger = logger;
            Task1 = task1;
        }

        public Task1 Task1 { get; }

        public async Task End()
        {
            logger.LogInformation("Log before scope. {step}", 1);
            using (var logScope = logger.BeginScope("guid: {interchangeId2}", Guid.NewGuid()))
            {
                logger.LogInformation("Log in scope, {step}", 2);
                await Task.Delay(100);
            }
            logger.LogInformation("Log after scope. {step}", 3);
        }
    }
}
