﻿using System;

namespace NETMP.Module11.LoggingAndMonitoring.Common
{
    public class SiteNodeCopiedToFileSystemEventArgs : EventArgs
    {
        public double SiteNodeNumber { get; }

        public double TotalSiteNodeNumbers { get; }

        public string SiteNodeUri { get; }

        public SiteNodeCopiedToFileSystemEventArgs(string uri, int number, int totalNumber)
        {
            SiteNodeNumber = number;
            TotalSiteNodeNumbers = totalNumber <= 0 ? 1 : totalNumber;
            SiteNodeUri = uri ?? string.Empty;
        }
    }
}
