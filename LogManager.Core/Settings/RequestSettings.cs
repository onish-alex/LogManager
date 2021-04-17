using System.Collections.Generic;

namespace LogManager.Core.Settings
{
    public class RequestSettings
    {
        public IEnumerable<string> AllowableExtensions { get; set; }

        public IEnumerable<string> HttpMethods { get; set; }

        public string DefaultUrl { get; set; }
    }
}
