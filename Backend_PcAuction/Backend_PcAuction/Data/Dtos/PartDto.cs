using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{
    public record PartDto(
        Guid Id,
        string Name,
        string? SpecificationValue1,
        string? SpecificationValue2,
        string? SpecificationValue3,
        string? SpecificationValue4,
        string? SpecificationValue5,
        string? SpecificationValue6,
        string? SpecificationValue7,
        string? SpecificationValue8,
        string? SpecificationValue9,
        string? SpecificationValue10,
        string CategoryId,
        Guid? SeriesId
    );

    public record CreatePartDto(
        [Required] string Name,
        string? SpecificationValue1,
        string? SpecificationValue2,
        string? SpecificationValue3,
        string? SpecificationValue4,
        string? SpecificationValue5,
        string? SpecificationValue6,
        string? SpecificationValue7,
        string? SpecificationValue8,
        string? SpecificationValue9,
        string? SpecificationValue10,
        Guid? SeriesId
    );

    public record UpdatePartDto(
        string Name,
        string? SpecificationValue1,
        string? SpecificationValue2,
        string? SpecificationValue3,
        string? SpecificationValue4,
        string? SpecificationValue5,
        string? SpecificationValue6,
        string? SpecificationValue7,
        string? SpecificationValue8,
        string? SpecificationValue9,
        string? SpecificationValue10,
        Guid? SeriesId
    );
}
