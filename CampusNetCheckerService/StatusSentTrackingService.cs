using System;
using System.IO;
using System.Threading.Tasks;

namespace CampusNetCheckerService
{
    public static class StatusSentTrackingService
    {
        private const string Path = "status.txt";

        public static async Task<SuccessStatus> GetStatus()
        {
            if (!File.Exists(Path))
            {
                await SetStatus(SuccessStatus.NotReported);
            }

            var file = await File.ReadAllTextAsync(Path);

            Enum.TryParse(file, out SuccessStatus status);
            return status;
        }

        public static async Task SetStatus(SuccessStatus status)
        {
            await File.WriteAllLinesAsync(Path, new[] { status.ToString() });
        }
    }

    public enum SuccessStatus
    {
        NotReported,
        Success,
        Error
    }
}
