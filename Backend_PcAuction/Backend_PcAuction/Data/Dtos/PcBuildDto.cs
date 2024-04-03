namespace Backend_PcAuction.Data.Dtos
{
    public record PcBuilderResultDto(

    );

    public record PcBuilderDataDto(
        string? MotherboardId,
        string? CpuId,
        string? GpuId,
        string? RamId,
        string? HddId,
        string? SsdId,
        string? PsuId,
        double Budget
    );
}
