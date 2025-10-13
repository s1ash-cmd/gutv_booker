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

    public async Task<(bool Success, EquipmentType? Type)> CreateEquipmentType(string name, string description,
        EquipmentType.EquipmentCategory category, bool osnova, string? attributesJson = null)
    {
        if (await _context.EquipmentTypes.AnyAsync(u => u.Name == name))
            return (false, null);

        var equipmentType = new EquipmentType
        {
            Name = name,
            Description = description,
            Category = category,
            AttributesJson = attributesJson ?? "{}",
            Osnova = osnova,
            Ronin = name.Contains("Ronin", StringComparison.OrdinalIgnoreCase)
        };

        _context.EquipmentTypes.Add(equipmentType);
        await _context.SaveChangesAsync();

        return (true, equipmentType);
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
        if (string.IsNullOrWhiteSpace(name)) return new List<EquipmentType>();
        name = name.ToLower();
        return await _context.EquipmentTypes.Where(u => u.Name.ToLower().Contains(name)).ToListAsync();
    }

    public async Task<List<EquipmentType>> GetEquipmentTypeByCategory(EquipmentType.EquipmentCategory category)
    {
        return await _context.EquipmentTypes.Where(e => e.Category == category).ToListAsync();
    }

    public async Task<List<EquipmentType>> GetAvailableToMe(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new List<EquipmentType>();

        IQueryable<EquipmentType> query = _context.EquipmentTypes.AsQueryable();

        if (!user.Osnova)
            query = query.Where(t => !t.Osnova);

        if (!user.Ronin)
            query = query.Where(t => !t.Ronin);

        return await query.ToListAsync();
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

    public async Task<(bool Success, EquipmentItem? Item)> CreateEquipmentItem(int equipmentTypeId, bool available)
    {
        var equipmentType = await _context.EquipmentTypes.FirstOrDefaultAsync(et => et.Id == equipmentTypeId);
        if (equipmentType == null) return (false, null);

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

        return (true, item);
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

    public async Task<(bool Success, EquipmentItemDto? ItemDto)> CreateEquipmentItemDto(int equipmentTypeId, bool available)
    {
        var (success, item) = await CreateEquipmentItem(equipmentTypeId, available);
        if (!success || item == null) return (false, null);

        return (true, new EquipmentItemDto
        {
            Id = item.Id,
            EquipmentTypeId = item.EquipmentTypeId,
            InventoryNumber = item.InventoryNumber,
            Available = item.Available
        });
    }
}
