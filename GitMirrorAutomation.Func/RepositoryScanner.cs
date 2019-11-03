using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace GitMirrorAutomation.Func
{
    public static class IntegrationTests
    {
        /// <summary>
        /// Will perform a scan to check if any new repositories exist.
        /// </summary>
        [FunctionName("scan")]
        public static Task ScanRepositoriesAsync(
          [TimerTrigger(Schedule.Every5Minutes)] TimerInfo timer,
          ILogger log,
          CancellationToken cancellationToken,
          ExecutionContext executionContext)
        {
            throw new NotImplementedException();
        }
    }
}
