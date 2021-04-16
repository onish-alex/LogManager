using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Whois.NET;

namespace LogManager.BLL.Utilities
{
    public class WebHelper
    {
        public static readonly string DefaultUrl = "http://tariscope.com";

        public async Task<string> GetOrganizationNameFromWhois(string ip)
        {
            //TODO check ip format

            var response = await WhoisClient.QueryAsync(ip);

            return response.OrganizationName;
        }

        public WebPageInfo MakeGetRequest(string filePath)
        {
            var client = new WebClient();
            var pageInBytes = client.DownloadData(DefaultUrl + filePath);
            var pageInString = client.Encoding.GetString(pageInBytes);

            string pageTitle = null;

            //TODO title fetching

            return new WebPageInfo()
            {
                Size = pageInBytes.Length,
                //Title = 
            };
        }

    }
}
