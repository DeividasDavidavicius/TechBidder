namespace Backend_PcAuction.Data.Entities
{
    public class PartPrice
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public double Price { get; set; }
        public DateTime PriceCheckDate { get; set; }
        public Part Part { get; set; }
    }
}
