using Microsoft.EntityFrameworkCore;

namespace Backend_PcAuction.Data.Entities
{
    public class PartRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Auction Auction { get; set; }
        [DeleteBehavior(DeleteBehavior.Restrict)]
        public Part Part { get; set; }
    }
}
