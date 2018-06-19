using System.Collections.Generic;

namespace NETMP.Module11.LoggingAndMonitoring.HttpManager.Interfaces
{
    public interface IHtmlCrawler
    {
        IEnumerable<string> FindHtmlPageLinks(string html);
    }
}
