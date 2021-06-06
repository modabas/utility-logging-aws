using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AWSLoggerTest.Classes
{
    public class Task1
    {
        private readonly ILogger<Task1> logger;
        private readonly Task2 task2;

        public Task1(ILogger<Task1> logger, Task2 task2)
        {
            this.logger = logger;
            this.task2 = task2;
        }

        public async Task Start()
        {
            logger.LogInformation("Log before scope. {step}", 1);
            using (var logScope = logger.BeginScope("guid: {interchangeId1}", Guid.NewGuid()))
            {
                logger.LogInformation("Log in scope, {step}", 2);
                await task2.End();
            }
            logger.LogInformation("Log after scope. {step}", 3);
        }
        public async Task End()
        {
            logger.LogInformation("Log before scope. {step}", 1);
            using (var logScope = logger.BeginScope("guid: {interchangeId1}", Guid.NewGuid()))
            {
                logger.LogInformation("Log in scope, {step}", 2);
            }
            logger.LogInformation("Log after scope. {step}", 3);
        }
    }
}
