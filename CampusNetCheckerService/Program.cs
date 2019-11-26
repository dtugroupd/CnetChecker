using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CampusNetCheckerService
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddHttpClient();
                    services.AddSingleton<ErrorReportingService, ErrorReportingService>();
                    services.AddSingleton<CampusNetService, CampusNetService>();
                });
    }
}
