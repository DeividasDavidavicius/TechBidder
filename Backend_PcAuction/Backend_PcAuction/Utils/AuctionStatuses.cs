namespace Backend_PcAuction.Utils
{
    public class AuctionStatuses
    {
        public const string New = nameof(New);
        public const string Active = nameof(Active);

        // NA = Not approved (auction which has a part which has not been approved by an admin yet)
        public const string NewNA = nameof(NewNA);
        public const string ActiveNA = nameof(ActiveNA);

        public const string EndedWithBids = nameof(EndedWithBids);
        public const string EndedWithoutBids = nameof(EndedWithoutBids);
        public const string Paid = nameof(Paid);
        public const string Cancelled = nameof(Cancelled);
        public const string PaymentNotReceived = nameof(PaymentNotReceived);
    }
}
