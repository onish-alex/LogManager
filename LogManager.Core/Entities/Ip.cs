using LogManager.Core.Utilities;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogManager.Core.Entities
{
    public class Ip : BaseEntity
    {
        public byte[] Address { get; set; }

        public string OwnerName { get; set; }
    
        [NotMapped]
        public long AddressAsLong => IpConverter.ToLong(Address);

        [NotMapped]
        public string AddressStr => IpConverter.FromBytes(Address);

    }
}
