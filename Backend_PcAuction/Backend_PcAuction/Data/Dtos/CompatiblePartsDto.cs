namespace Backend_PcAuction.Data.Dtos
{
    public class CompatiblePartsDto
    {
        public record CompatiblePartsDataDto(
            string CategoryId,
            Guid PartId,
            string CompatibleCategoryId
        );

        public record CompatiblePartsResultDto(
            Guid PartId,
            string PartName,
            int ActiveAuctionForPartCnt
        );
    }
}
