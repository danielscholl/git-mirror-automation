using GitMirrorAutomation.Logic;
using GitMirrorAutomation.Logic.Config;
using GitMirrorAutomation.Logic.Storage;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GitMirrorAutomation.Func
{
    public static class Functions
    {
        /// <summary>
        /// Will perform a scan to check if any new repositories exist.
        /// </summary>
        [FunctionName("scan")]
        public static async Task ScanRepositoriesAsync(
          [TimerTrigger(Schedule.Every5Minutes, RunOnStartup = true)] TimerInfo timer,
          ILogger log,
          CancellationToken cancellationToken)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? throw new ArgumentNullException("Storage connection string not set");
            var storageProvider = new AzureBlobStorageProvider(connectionString, "configuration");

            var configurations = await LoadConfigFilesAsync(storageProvider, cancellationToken);
            var scanner = new ConfigurationProcessor(log);

            foreach (var cfg in configurations)
            {
                log.LogInformation($"Processing source {cfg.Source}");
                await scanner.ProcessAsync(cfg, cancellationToken);
            }
        }

        private static async Task<Configuration[]> LoadConfigFilesAsync(AzureBlobStorageProvider storageProvider, CancellationToken cancellationToken)
        {
            var files = await storageProvider.ListAsync("", cancellationToken);
            var configs = new List<Configuration>();
            foreach (var file in files)
            {
                if (!file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                configs.Add(ParseConfiguration(await storageProvider.GetAsync(file, cancellationToken)));
            }
            return configs.ToArray();
        }

        private static Configuration ParseConfiguration(string json)
            => JsonSerializer.Deserialize<Configuration>(json, JsonSettings.Default);
    }
}
