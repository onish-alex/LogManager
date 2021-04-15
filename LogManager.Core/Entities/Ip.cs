namespace LogManager.Core.Entities
{
    public class Ip : BaseEntity
    {
        public byte[] Address { get; set; }

        public string OwnerName { get; set; }
    }
}
