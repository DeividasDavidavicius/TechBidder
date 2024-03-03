namespace Backend_PcAuction.Data.Entities
{
    public class Bid
    {
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public DateTime CreationDate { get; set; }
        public Auction Auction { get; set; }
    }
}
