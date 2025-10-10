using gutv_booker.Data;
using gutv_booker.Models;
using Microsoft.EntityFrameworkCore;

namespace gutv_booker.Services;

public class EquipmentService
{
    private readonly AppDbContext _context;

    public EquipmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EquipmentType> CreateEquipmentType(string name, string description,
        EquipmentType.EquipmentCategory category, string? attributesJson = null)
    {
        if (await _context.EquipmentTypes.AnyAsync(u => u.Name == name))
            throw new InvalidOperationException($"Оборудование '{name}' уже существует");

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

    public async Task<List<EquipmentType>> GetAllEquipmentTypes()
    {
        return await _context.EquipmentTypes.ToListAsync();
    }

    public async Task<EquipmentType?> GetEquipmentTypeById(int id)
    {
        return await _context.EquipmentTypes.FindAsync(id);
    }

    public async Task<List<EquipmentType>> GetEquipmentTypeByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<EquipmentType>();

        name = name.ToLower();

        return await _context.EquipmentTypes.Where(u => u.Name.ToLower().Contains(name)).ToListAsync();
    }

    public async Task<List<EquipmentType>> GetEquipmentTypeByCategory(EquipmentType.EquipmentCategory category)
    {
        return await _context.EquipmentTypes.Where(e => e.Category == category).ToListAsync();
    }

    public async Task<bool> UpdateEquipmentType(int id, string? name = null, string? description = null,
        EquipmentType.EquipmentCategory? category = null, string? attributesJson = null)
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

    public async Task<bool> DeleteEquipmentType(int id)
    {
        var equipmentType = await _context.EquipmentTypes.FindAsync(id);
        if (equipmentType == null) return false;

        _context.EquipmentTypes.Remove(equipmentType);
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<EquipmentItem> CreateEquipmentItem(int equipmentTypeId, bool available)
    {
        var equipmentType = await _context.EquipmentTypes.FirstOrDefaultAsync(et => et.Id == equipmentTypeId);

        if (equipmentType == null)
            throw new InvalidOperationException($"EquipmentType с Id {equipmentTypeId} не найден");

        int categoryCode = (int)equipmentType.Category;

        var countForType = await _context.EquipmentItems.CountAsync(e => e.EquipmentTypeId == equipmentTypeId);

        var inventoryNumber = $"{categoryCode}-{equipmentTypeId + 0:D3}-{countForType + 1:D2}";

        var item = new EquipmentItem
        {
            EquipmentTypeId = equipmentTypeId,
            InventoryNumber = inventoryNumber,
            Available = available
        };

        _context.EquipmentItems.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<List<EquipmentItem>> GetAllEquipmentItems()
    {
        return await _context.EquipmentItems.ToListAsync();
    }

    public async Task<EquipmentItem?> GetEquipmentItemById(int id)
    {
        return await _context.EquipmentItems.FindAsync(id);
    }

    public async Task<List<EquipmentItem>> GetEquipmentItemsByType(int equipmentTypeId)
    {
        return await _context.EquipmentItems.Where(e => e.EquipmentTypeId == equipmentTypeId).ToListAsync();
    }

    public async Task<bool> DeleteEquipmentItem(int id)
    {
        var equipmentItem = await _context.EquipmentItems.FindAsync(id);
        if (equipmentItem == null) return false;

        _context.EquipmentItems.Remove(equipmentItem);
        await _context.SaveChangesAsync();
        return true;
    }
}