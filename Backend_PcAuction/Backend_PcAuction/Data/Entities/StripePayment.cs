namespace Backend_PcAuction.Data.Entities
{
    public class StripePayment
    {
        public string Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public Purchase Purchase { get; set; }
    }
}
