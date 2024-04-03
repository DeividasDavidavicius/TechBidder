using Backend_PcAuction.Data.Entities;

namespace Backend_PcAuction.Utils
{
    public class PcBuild
    {
        public Part Motherboard { get; set; }
        public Part CPU { get; set; }
        public Part GPU { get; set; }
        public Part RAM { get; set; }
        public Part SSD { get; set; }
        public Part HDD { get; set; }
        public Part PSU { get; set; }
        public double TotalPrice { get; set; }
    }
}
