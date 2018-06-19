using System.Collections.Generic;
using CsQuery;
using NETMP.Module11.LoggingAndMonitoring.HttpManager.Interfaces;

namespace NETMP.Module11.LoggingAndMonitoring.HttpManager
{
    public class HtmlCrawler : IHtmlCrawler
    {
        public IEnumerable<string> FindHtmlPageLinks(string html)
        {
            var csQuery = CQ.Create(html);

            foreach (var domObject in csQuery.Find("a"))
            {
                yield return domObject.GetAttribute("href");
            }
        }
    }
}
