using gutv_booker.Data;
using gutv_booker.Models;

namespace gutv_booker.Services;
public class EquipmentService
{
    private readonly AppDbContext _context;

    public EquipmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EquipmentType> CreateEquipmentType(
        string name,
        string description,
        EquipmentType.EquipmentCategory category,
        Dictionary<string, object>? attributes = null)
    {
        var equipmentType = new EquipmentType
        {
            Name = name,
            Description = description,
            Category = category,
            Attributes = attributes ?? new Dictionary<string, object>()
        };

        _context.EquipmentTypes.Add(equipmentType);
        await _context.SaveChangesAsync();
        return equipmentType;
    }



}