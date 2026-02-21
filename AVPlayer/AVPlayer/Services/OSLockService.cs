using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace AVPlayer.Services
{
    public interface IOSLockService
    {
        void PreventSleep();
        void AllowSleep();
    }

    public class OSLockService : IOSLockService
    {
        private readonly ILogger<OSLockService> _logger;

        public OSLockService(ILogger<OSLockService> logger)
        {
            _logger = logger;
        }

        [Flags]
        private enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        public void PreventSleep()
        {
            try
            {
                // Prevent system sleep and display sleep
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
                _logger.LogInformation("OS Sleep Prevented.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to prevent OS sleep.");
            }
        }

        public void AllowSleep()
        {
             try
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                _logger.LogInformation("OS Sleep Allowed.");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to restore OS sleep state.");
            }
        }
    }
}
