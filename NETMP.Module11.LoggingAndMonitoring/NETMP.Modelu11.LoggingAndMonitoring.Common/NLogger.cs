using NLog;

namespace NETMP.Module11.LoggingAndMonitoring.Common
{
    public static class NLogger
    {
        public static ILogger Logger = LogManager.GetCurrentClassLogger();
    }
}
