using System;

namespace LogManager.Core.Entities
{
    public class LogEntry : BaseEntity
    {
        public virtual Ip IpInfo { get; set; }

        public virtual File FileInfo { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Method { get; set; }

        public short StatusCode { get; set; }

        public int Amount { get; set; }
    }
}
