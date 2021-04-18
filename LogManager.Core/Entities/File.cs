namespace LogManager.Core.Entities
{
    public class File : BaseEntity
    {
        public string Path { get; set; }

        public string Title { get; set; }

        public int? Size { get; set; } 
    }
}
