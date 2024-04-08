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
        bool MotherboardAlreadyHave,
        bool CpuAlreadyHave,
        bool GpuAlreadyHave,
        bool RamAlreadyHave,
        bool SsdAlreadyHave,
        bool HddAlreadyHave,
        bool IncludePsu,
        double Budget
    );
}
