using AWSLoggerTest.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Mod.Utility.Logging.Aws.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Setup().Build();

            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();

                try
                {
                    var myService = services.GetRequiredService<Task1>();
                    await myService.Start();

                    Console.WriteLine();
                    Console.WriteLine("Completed!");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occured!");
                }
            }
        }

        private static IHostBuilder Setup()
        {
            return new HostBuilder()
                //Setup configuration files
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Path.Combine(AppContext.BaseDirectory))
                        .AddJsonFile("appsettings.json", false, true);
                })
                //Add maintenance tasks to use as entrypoints
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddTransient<Task1>();
                    services.AddTransient<Task2>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddAwsLogging(hostingContext.Configuration.GetAwsLoggingConfigSection());
                });
        }

    }
}
