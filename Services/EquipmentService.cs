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

    public async Task<EquipmentType> CreateEquipmentType(string name, string description, EquipmentType.EquipmentCategory category, string? attributesJson = null)
    {
        var equipmentType = new EquipmentType
        {
            Name = name,
            Description = description,
            Category = category,
            AttributesJson = attributesJson ?? "{}"
        };

        _context.EquipmentTypes.Add(equipmentType);
        await _context.SaveChangesAsync();
        return equipmentType;
    }

    public async Task<bool> DeleteEquipmentType(int id)
    {
        var equipmentType = await _context.EquipmentTypes.FindAsync(id);
        if (equipmentType == null) return false;

        _context.EquipmentTypes.Remove(equipmentType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateEquipmentType(int id, string? name = null, string? description = null, EquipmentType.EquipmentCategory? category = null, string? attributesJson = null)
    {
        var equipmentType = await _context.EquipmentTypes.FindAsync(id);
        if (equipmentType == null) return false;

        if (!string.IsNullOrEmpty(name)) equipmentType.Name = name;
        if (!string.IsNullOrEmpty(description)) equipmentType.Description = description;
        if (category.HasValue) equipmentType.Category = category.Value;
        if (!string.IsNullOrEmpty(attributesJson)) equipmentType.AttributesJson = attributesJson;

        _context.EquipmentTypes.Update(equipmentType);
        await _context.SaveChangesAsync();
        return true;
    }

}