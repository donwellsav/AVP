using Serilog;
using System.IO;

namespace AVP.Services;

public static class SerilogFactory
{
    public static ILogger CreateLogger()
    {
        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AVP", "Logs", "log-.txt");

        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();
    }
}
