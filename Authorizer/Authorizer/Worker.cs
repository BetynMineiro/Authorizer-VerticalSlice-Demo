using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Authorizer.Strategies;

namespace Authorizer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IProcessFileStrategy _processStrategy;

        public Worker(ILogger<Worker> logger, IProcessFileStrategy processFileStrategy)
        {
            _logger = logger;
            _processStrategy = processFileStrategy;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            var basePathInput = Path.Combine(currentDirectory, "input");
            var basePathOutput = Path.Combine(currentDirectory, "output");

            _logger.LogInformation("Worker Authorizer start at: {time}", DateTimeOffset.Now);

            if(!Directory.Exists(basePathInput))
            Directory.CreateDirectory(basePathInput);

            if (!Directory.Exists(basePathOutput))
                Directory.CreateDirectory(basePathOutput);


            while (!stoppingToken.IsCancellationRequested)
            {

                string fullInputPath = Path.Combine(basePathInput, "operations");

                if (File.Exists(fullInputPath)) {

                    using (StreamReader file = new StreamReader(fullInputPath))
                    {
                        var listOutput = new List<string>() { };
                        _logger.LogInformation("{time} - Start read operations file", DateTimeOffset.UtcNow);
                        while (!file.EndOfStream)
                        {
                            string input = file.ReadLine();
                            var strategy = _processStrategy.GetProcessLineStrategy(input);
                            var output = await strategy.ProcessAsync().ConfigureAwait(false);
                            _logger.LogInformation("{time} - Output: {item}", DateTimeOffset.UtcNow, output);
                            listOutput.Add(output);
                        }

                        string fullOutputPath = Path.Combine(basePathOutput, $"{DateTimeOffset.UtcNow.Ticks}-operations-processed");
                        File.WriteAllLines(fullOutputPath, listOutput);
                        File.Delete(fullInputPath);
                    }
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
