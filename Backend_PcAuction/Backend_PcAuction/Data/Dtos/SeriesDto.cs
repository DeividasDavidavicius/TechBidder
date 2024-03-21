using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record SeriesDto(Guid Id, string Name, string CategoryId);
    public record CreateSeriesDto([Required] string Name);
    public record UpdateSeriesDto([Required] string Name);
}
