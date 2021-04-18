using System;
using System.Net;
using System.Threading.Tasks;
using Whois.NET;
using HtmlAgilityPack;
using LogManager.Core.Settings;
using Microsoft.Extensions.Options;

namespace LogManager.BLL.Utilities
{
    public class WebHelper
    {
        private RequestSettings requestSettings;
        
        public WebHelper(IOptionsSnapshot<RequestSettings> requestSettingsSnapshot)
        {
            this.requestSettings = requestSettingsSnapshot.Value;
        }

        public string GetOrganizationNameByWhois(string ip)
        {
            var response = WhoisClient.Query(ip);
            return response.OrganizationName;
        }

        public WebPageInfo GetPageInfo(string filePath)
        {
            var client = new WebClient();

            string pageContent;

            try
            {
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
