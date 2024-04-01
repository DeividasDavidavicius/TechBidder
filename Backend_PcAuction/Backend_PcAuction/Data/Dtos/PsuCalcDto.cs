using System;

namespace Backend_PcAuction.Data.Dtos
{
    public record PsuCalcResultDto(
        double CalculatedWattage,
        double RecommendedWattage
    );

    public record PsuCalcDataDto(
        Guid? MotherboardId,
        Guid? CpuId,
        Guid? GpuId,
        Guid? RamId,
        Guid? HddId,
        Guid? SsdId
    );
}
