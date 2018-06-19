namespace NETMP.Module11.LoggingAndMonitoring.Common
{
    public class SiteNode
    {
        public string Uri { get; }

        public string Html { get; set; }

        public byte[] Media { get; set; }

        public SiteNode() { }

        public SiteNode(string uri)
        {
            Uri = uri;
        }
    }
}
