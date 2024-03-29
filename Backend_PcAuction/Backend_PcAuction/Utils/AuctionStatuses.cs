namespace Backend_PcAuction.Utils
{
    public class AuctionStatuses
    {
        public const string New = nameof(New);
        public const string Active = nameof(Active);
        public const string NewNA = nameof(NewNA); // NA = Not approved (part which has not been approved by an admin yet)
        public const string ActiveNA = nameof(ActiveNA);
    }
}
