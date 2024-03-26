namespace Backend_PcAuction.Data.Entities
{
    // Has two types: EbayAverage | SoldOnSite
    // Save SoldOnSite when auction ends and it has bids (or maybe when someone actually "pays" for the part
    public class PartPrice
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public double Price { get; set; }
        public DateTime PriceCheckDate { get; set; }
        public Part Part { get; set; }
    }
}
