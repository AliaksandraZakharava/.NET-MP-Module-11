using System;
using NETMP.Module11.LoggingAndMonitoring.CopyingManager;
using NETMP.Module11.LoggingAndMonitoring.DialogManager.Console;
using NETMP.Module11.LoggingAndMonitoring.DialogManager.Interfaces;
using NETMP.Module11.LoggingAndMonitoring.HttpManager;
using NETMP.Module11.LoggingAndMonitoring.HttpManager.Interfaces;

namespace NETMP.Module11.LoggingAndMonitoring.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IInputOutputManager inputOutputManger = new InputOutputManager();
            IUserDialogManager dialogManager = new UserDialogManager(inputOutputManger);

            IHttpResponseProvider httpResponseProvider = new HttpResponseProvider();
            IHtmlCrawler htmlCrawler = new HtmlCrawler();

            SiteCopyingManager siteCopyingManager = new SiteCopyingManager(httpResponseProvider, htmlCrawler);

            try
            {
                //var outputPath = dialogManager.GetInputDirectoryPath();
                //var siteUri = dialogManager.GetSiteUri();
                //var depth = dialogManager.GetDepthNumber();
                //var limits = dialogManager.GetTransitionToOtherDomainsLimits();
                //var extensionsToExclude = dialogManager.GetFileExtensionsToExclude();

                siteCopyingManager.SiteNodeFounded += dialogManager.DisplaySiteNodeFoundMessage;
                siteCopyingManager.SiteNodeCopiedToFileSystem += dialogManager.DisplaySiteNodeCopiedToFileSystemStepCompletedMessage;

                //siteCopyingManager.CopySite(siteUri, outputPath, depth, limits, extensionsToExclude);
                siteCopyingManager.CopySite("https://news.tut.by/top5news/", @"D:\ExclusiveTutBy", 1);
            }
            catch (Exception exc)
            {
                dialogManager.DisplayExceptionMessage(exc);
            }
            finally
            {
                dialogManager.DisplayOperationFinishedMessage();
                dialogManager.DisplayWaitMessage();
            }
        }
    }
}
