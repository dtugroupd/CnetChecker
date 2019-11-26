using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CampusNetCheckerService
{
    public class ErrorReportingService
    {
        private readonly ILogger<ErrorReportingService> _logger;

        public ErrorReportingService(ILogger<ErrorReportingService> logger)
        {
            _logger = logger;
        }

        public async Task ReportError(CampusNetException ex)
        {
            _logger.LogInformation("Reporting error {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
            await Task.Run(() => { });
        }
    }
}
