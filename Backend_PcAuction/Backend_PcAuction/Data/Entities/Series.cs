namespace Backend_PcAuction.Data.Entities
{
    public class Series
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PartCategory PartCategory { get; set; }
    }
}
