using System;
using System.Collections.Generic;
using System.Linq;
using NETMP.Module11.LoggingAndMonitoring.Common;
using NETMP.Module11.LoggingAndMonitoring.CopyingManager.Helpers;
using NETMP.Module11.LoggingAndMonitoring.HttpManager.Interfaces;

namespace NETMP.Module11.LoggingAndMonitoring.CopyingManager
{
    public class SiteCopyingManager
    {
        private readonly IHttpResponseProvider _httpResponseProvider;
        private readonly IHtmlCrawler _htmlCrawler;

        public event EventHandler<string> SiteNodeFounded;
        public event EventHandler<SiteNodeCopiedToFileSystemEventArgs> SiteNodeCopiedToFileSystem;

        private readonly bool _verboseOn;
        private string _rootUri;

        public SiteCopyingManager(IHttpResponseProvider httpResponseProvider, IHtmlCrawler htmlCrawler, bool turnVerboseOn = true)
        {
            _httpResponseProvider = httpResponseProvider ?? throw new ArgumentNullException(nameof(httpResponseProvider));
            _htmlCrawler = htmlCrawler ?? throw new ArgumentNullException(nameof(_htmlCrawler));

            _verboseOn = turnVerboseOn;
        }

        public void CopySite(string uri, string outPath, int depthLevel,
                             TransitionToOtherDomainsLimits transactionLimits = TransitionToOtherDomainsLimits.NoLimits,
                             List<string> excludeFileExtensions = null)
        {
            if (string.IsNullOrEmpty(uri))
            {
                NLogger.Logger.Error($"Null or empty passed argument: nameof {uri}");

                throw new ArgumentException("Null or empty uri.");
            }

            if (depthLevel < 0)
            {
                NLogger.Logger.Error($"Negative depth level value: nameof {depthLevel}, value {depthLevel}");

                throw new ArgumentException("Negative depth level value.");
            }

            if (excludeFileExtensions != null)
            {
                ImageExtensionsHelper.ExcludeImageExtensions(excludeFileExtensions);
            }

            _rootUri = uri;

            NLogger.Logger.Info($"Starting copying site. Passed uri: {uri}");

            NLogger.Logger.Info("Starting getting site nodes...");

            var siteNodes = GetAllSiteNodes(uri, depthLevel, transactionLimits).ToList();

            NLogger.Logger.Info("Finished getting site nodes.");

            NLogger.Logger.Info($"Starting writing site nodes to file system. Output path: {outPath}");

            WriteSiteNodesToFileSystem(siteNodes, outPath);

            NLogger.Logger.Info("Successfully finished copying site.");
        }

        #region Private methods

        private IEnumerable<SiteNode> GetAllSiteNodes(string rootUri, int depthLevel, TransitionToOtherDomainsLimits transactionLimits)
        {
            while (depthLevel >= 0)
            {
                depthLevel--;

                var rootNode = SiteNodeHelper.GetFilledSiteNode(rootUri, _httpResponseProvider);

                OnSiteNodeFounded(this, rootNode.Uri);

                yield return rootNode;

                var links = SiteNodeHelper.GetSiteNodeLinks(_rootUri, rootNode, transactionLimits, _htmlCrawler);

                foreach (var link in links)
                {
                    var absoluteLink = UriHelper.GetAbsoluteLink(link, rootNode.Uri);

                    var linkNodes = GetAllSiteNodes(absoluteLink, depthLevel, transactionLimits);

                    foreach (var node in linkNodes)
                    {
                        NLogger.Logger.Info($"Node found: uri {node.Uri}");

                        yield return node;
                    }
                }
            }
        }

        private void WriteSiteNodesToFileSystem(IEnumerable<SiteNode> siteNodes, string outPath)
        {
            NLogger.Logger.Info("Successfully writing site nodes to file system...");

            FileSystemHelper.CreateDirectory(outPath);

            var totalNodesCount = siteNodes.Count();
            var nodeNumber = 1;

            foreach (var node in siteNodes)
            {
                try
                {
                    var uriParts = SiteNodeHelper.GetSiteNodeUriParts(node);

                    var writePath = SiteNodeHelper.CreateAndGetWritePathForAUri(uriParts, outPath);

                    SiteNodeHelper.WriteSiteNodeToFileSystem(node, writePath);

                    OnSiteNodeCopiedToFileSystem(this, new SiteNodeCopiedToFileSystemEventArgs(node.Uri, nodeNumber, totalNodesCount));

                    NLogger.Logger.Info($"Copied a site node with uri {node.Uri}");
                }
                catch(InvalidOperationException exc)
                {
                    NLogger.Logger.Error($"Copying a site node with uri {node.Uri} finished with exception: {exc.Message}");

                    OnSiteNodeCopiedToFileSystem(this, new SiteNodeCopiedToFileSystemEventArgs(exc.Message, nodeNumber, totalNodesCount));
                }
                finally
                {
                    nodeNumber++;
                }
            }

            NLogger.Logger.Info("Successfully finished writing site nodes to file system.");
        }

        private void OnSiteNodeFounded(object sender, string e)
        {
            if (_verboseOn)
            {
                var handler = SiteNodeFounded;

                handler?.Invoke(sender, e);
            }
        }

        private void OnSiteNodeCopiedToFileSystem(object sender, SiteNodeCopiedToFileSystemEventArgs e)
        {
            if (_verboseOn)
            {
                var handler = SiteNodeCopiedToFileSystem;

                handler?.Invoke(sender, e);
            }
        }

        #endregion
    }
}
