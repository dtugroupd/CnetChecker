using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CampusNetCheckerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _config;
        private readonly CampusNetService _cnService;
        private readonly ErrorReportingService _errorService;

        public Worker(ILogger<Worker> logger, IConfiguration config, CampusNetService cnService, ErrorReportingService errorService)
        {
            _logger = logger;
            _config = config;
            _cnService = cnService;
            _errorService = errorService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    await _cnService.FetchLimitedPassword(_config["CN:Username"], _config["CN:Password"]);
                    await _errorService.ReportSuccess();
                }
                catch (Exception ex)
                {
                    if (ex is CampusNetException cex)
                    {
                        _logger.LogWarning("A known error occured. {ErrorTitle}: {ErrorMessage}", cex.Title, cex.Message);
                        await _errorService.ReportError(cex);
                    }
                    else
                    {
                        _logger.LogCritical("Login request failed with exception {ExceptionName}: {ExceptionMessage}", ex.GetType().Name, ex.Message);
                    }
                }

                await Task.Delay(Convert.ToInt32(_config["Timeout"]), stoppingToken);
            }
        }
    }
}
