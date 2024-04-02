namespace Backend_PcAuction.Data.Dtos
{
    public record PartPriceDto(Guid PartId, double AveragePrice);
    public record CreatePartPriceDto(double Price);
}
