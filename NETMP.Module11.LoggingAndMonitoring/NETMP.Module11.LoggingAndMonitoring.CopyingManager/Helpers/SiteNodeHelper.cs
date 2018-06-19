using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NETMP.Module11.LoggingAndMonitoring.Common;
using NETMP.Module11.LoggingAndMonitoring.CopyingManager.PerformanceCounters;
using NETMP.Module11.LoggingAndMonitoring.HttpManager.Interfaces;
using PerformanceCounterHelper;

namespace NETMP.Module11.LoggingAndMonitoring.CopyingManager.Helpers
{
    public static class SiteNodeHelper
    {
        private const string UrlSchemeSeparator = @"//";
        private const string HtmlFileExtension = ".html";
        private const string UriWithoutParamsRegexString = @"[^\?]+\?";
        private const string UriWithoutTagsRegexString = @".+\#";
        private const string UriWithoutMailToRegexString = @".+(?=mail)";

        private static readonly CounterHelper<Counters> _performanceHelper;

        static SiteNodeHelper()
        {
            _performanceHelper = PerformanceHelper.CreateCounterHelper<Counters>("Test");

            _performanceHelper.RawValue(Counters.SuccessfullHttpRequests, 0);
            _performanceHelper.RawValue(Counters.FailedHttpRequests, 0);
        }

        public static SiteNode GetFilledSiteNode(string rootUri, IHttpResponseProvider httpResponseProvider)
        {
            var rootNode = new SiteNode(rootUri);

            if (UriHelper.IsMediaFile(rootUri))
            {
                try
                {
                    rootNode.Media = httpResponseProvider.RequestLinkBytes(rootUri);
                    _performanceHelper.Increment(Counters.SuccessfullHttpRequests);
                }
                catch
                {
                    rootNode.Media = new byte[0];
                    _performanceHelper.Increment(Counters.FailedHttpRequests);
                }
            }
            else
            {
                try
                {
                    rootNode.Html = httpResponseProvider.RequestHttpLayout(rootUri);
                    _performanceHelper.Increment(Counters.SuccessfullHttpRequests);
                }
                catch
                {
                    rootNode.Html = string.Empty;
                    _performanceHelper.Increment(Counters.FailedHttpRequests);
                }
            }

            NLogger.Logger.Info($"{rootUri} has been handled.");

            return rootNode;
        }

        public static IEnumerable<string> GetSiteNodeLinks(string rootUri, SiteNode node, TransitionToOtherDomainsLimits transactionLimits, IHtmlCrawler htmlCrawler)
        {
            var links = htmlCrawler.FindHtmlPageLinks(node.Html).Where(UriHelper.IsValidLink);

            var result = FilterLinksAccordingToTransitionToOtherDomainsLimits(rootUri, links, transactionLimits);

            NLogger.Logger.Info($"{result.Count()} links found for uri {node.Uri}");

            return result;
        }

        public static IEnumerable<string> GetSiteNodeUriParts(SiteNode node)
        {
            return node.Uri.Split(new[] { UrlSchemeSeparator }, StringSplitOptions.None).Last().Split('/')
                           .Where(part => !string.IsNullOrEmpty(part))
                           .Select(part => !part.Contains("?") ? part : Regex.Match(part, UriWithoutParamsRegexString).Value.TrimEnd('?'))
                           .Select(part => !part.Contains("#") ? part : Regex.Match(part, UriWithoutTagsRegexString).Value.TrimEnd('#'))
                           .Select(part => !part.Contains("mailto") ? part : Regex.Match(part, UriWithoutMailToRegexString).Value)
                           .ToList();
        }

        public static string CreateAndGetWritePathForAUri(IEnumerable<string> uriParts, string outPath)
        {
             var writePath = outPath;

            foreach (var uriPart in uriParts)
            {
                writePath = Path.Combine(writePath, uriPart);

                if (!uriPart.EndsWith(HtmlFileExtension))
                {
                    FileSystemHelper.CreateDirectory(writePath);

                    NLogger.Logger.Info($"Directory {writePath} has been created.");
                }
            }

            if (!FileSystemHelper.HasExtension(writePath) ||
                 FileSystemHelper.GetExtension(writePath) != HtmlFileExtension)
            {
                if (!ImageExtensionsHelper.SupportedImageExtensions.Contains(FileSystemHelper.GetExtension(writePath)))
                {
                    writePath = Path.Combine(writePath, $"{uriParts.Last()}{HtmlFileExtension}");
                }
            }

            return writePath;
        }

        public static void WriteSiteNodeToFileSystem(SiteNode node, string writePath)
        {
            File.Create(writePath).Close();

            if (node.Media != null)
            {
                FileSystemHelper.CreateImageFile(writePath, node.Media);

                NLogger.Logger.Info($"Image has been copied to {writePath}");
            }
            else if (!string.IsNullOrEmpty(node.Html))
            {
                FileSystemHelper.CreateTextFile(writePath, node.Html);

                NLogger.Logger.Info($"Html page has been copied to {writePath}.");
            }
            else
            {
                NLogger.Logger.Error($"Strange link can not be handled (-_-): {node.Uri}");

                throw new InvalidOperationException($"Strange link (-_-): {node.Uri}");
            }
        }

        #region Private methods

        private static IEnumerable<string> FilterLinksAccordingToTransitionToOtherDomainsLimits(string rootUri, IEnumerable<string> links, TransitionToOtherDomainsLimits transactionLimits)
        {
            switch (transactionLimits)
            {
                case TransitionToOtherDomainsLimits.NoLimits:
                    return links;
                case TransitionToOtherDomainsLimits.OnlyInsideCurrentDomain:
                    return links.Where(link => link.Contains(UriHelper.GetHost(rootUri)));
                case TransitionToOtherDomainsLimits.NotHigherThenPassedUri:
                    return links.Where(link => link.Contains(rootUri));
                default:
                    return Enumerable.Empty<string>();
            }
        }

        #endregion
    }
}
