using gutv_booker.Data;
using gutv_booker.Models;
using Microsoft.EntityFrameworkCore;

namespace gutv_booker.Services;

public class ReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EquipmentTypeReportDto>> GetEquipmentReport()
    {
        var result = await _context.EquipmentTypes
            .Include(t => t.EquipmentItems)
            .Select(t => new EquipmentTypeReportDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category.ToString(),
                Osnova = t.Osnova,
                Ronin = t.Ronin,
                Attributes = t.Attributes,
                Items = t.EquipmentItems.Select(i => new EquipmentItemReportDto
                {
                    Id = i.Id,
                    InventoryNumber = i.InventoryNumber,
                    Available = i.Available,
                    TypeName = t.Name,
                    TypeCategory = t.Category.ToString()
                }).ToList()
            })
            .ToListAsync();
        return result;
    }
}
