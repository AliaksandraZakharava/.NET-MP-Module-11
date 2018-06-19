using System.Diagnostics;
using PerformanceCounterHelper;

namespace NETMP.Module11.LoggingAndMonitoring.CopyingManager.PerformanceCounters
{
    [PerformanceCounterCategory("Module10.Http.CopyingManagerCategory", 
                                PerformanceCounterCategoryType.MultiInstance,
                                "Module10.Http.CopyingManagerCategory counter")]
    public enum Counters
    {
        [PerformanceCounter("SuccessfullHttpRequests", "SuccessfullHttpRequests", PerformanceCounterType.NumberOfItems32)]
        SuccessfullHttpRequests,

        [PerformanceCounter("FailedHttpRequests", "FailedHttpRequests", PerformanceCounterType.NumberOfItems32)]
        FailedHttpRequests
    }
}
