using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Utils;

namespace Backend_PcAuction.Services
{
    public interface ICalculationsService
    {
        PsuCalcResultDto CalculatePSU(Part motherboard, Part cpu, Part gpu, Part ram, Part ssd, Part hdd);
    }

    public class CalculationsService : ICalculationsService
    {
        public PsuCalcResultDto CalculatePSU(Part motherboard, Part cpu, Part gpu, Part ram, Part ssd, Part hdd)
        {
            List<Part> parts = new List<Part>();
            double psuSize = 0;

            parts.Add(motherboard);
            parts.Add(cpu);
            parts.Add(gpu);
            parts.Add(ram);
            parts.Add(ssd);
            parts.Add(hdd);

            foreach(Part part in parts)
            {
                if(part != null)
                {
                    if(part.Category.Id == PartCategories.RAM)
                    {
                        psuSize += Double.Parse(part.SpecificationValue10) * Int32.Parse(part.SpecificationValue4);
                    }
                    psuSize += Double.Parse(part.SpecificationValue10);
                }
            }

            double calculatedWattage = Math.Ceiling(psuSize / 50) * 50 + 50;
            double recommendedWattage = -1;

            if (gpu != null)
            {
                recommendedWattage = Double.Parse(gpu.SpecificationValue9);
            }

            return new PsuCalcResultDto(calculatedWattage, recommendedWattage);
        }
    }
}
