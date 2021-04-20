using System;
using System.Net;
using Whois.NET;
using HtmlAgilityPack;
using LogManager.Core.Settings;
using Microsoft.Extensions.Options;

namespace LogManager.BLL.Utilities
{
    public class WebHelper
    {
        private RequestSettings requestSettings;
        private WebClient client;
        private object locker = new object();

        public WebHelper(IOptions<RequestSettings> requestSettingsSnapshot)
        {
            this.requestSettings = requestSettingsSnapshot.Value;
            this.client = new WebClient();
        }

        public string GetOrganizationNameByWhois(string ip)
        {
            var response = WhoisClient.Query(ip);
            return response.OrganizationName;
        }

        public WebPageInfo GetPageInfo(string filePath)
        {
            string pageContent;

            try
            {
                lock (locker)
                    pageContent = client.DownloadString(new Uri(requestSettings.DefaultUrl + filePath));
            }
            catch
            {
                throw;
            }

            var pageSize = client.Encoding.GetByteCount(pageContent);

            var document = new HtmlDocument();
            document.LoadHtml(pageContent);
            var titleNode = document.DocumentNode.SelectSingleNode("//head/title");

            var pageTitle = titleNode?.InnerText ?? string.Empty;

            return new WebPageInfo()
            {
                Size = pageSize,
                Title = pageTitle
            };
        }
    }
}
