namespace Backend_PcAuction.Data.Entities
{
    public class Part
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? SpecificationValue1 { get; set; }
        public string? SpecificationValue2 { get; set; }
        public string? SpecificationValue3 { get; set; }
        public string? SpecificationValue4 { get; set; }
        public string? SpecificationValue5 { get; set; }
        public string? SpecificationValue6 { get; set; }
        public string? SpecificationValue7 { get; set; }
        public string? SpecificationValue8 { get; set; }
        public string? SpecificationValue9 { get; set; }
        public string? SpecificationValue10 { get; set; }
        public PartCategory Category { get; set; }
        public Series? Series { get; set; }
    }
}
