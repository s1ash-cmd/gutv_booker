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

    public EqModelResponseDto EqModelToResponseDto(EquipmentModel eqModel) => new EqModelResponseDto
    {
        Id = eqModel.Id,
        Name = eqModel.Name,
        Description = eqModel.Description,
        Category = eqModel.Category,
        Access = eqModel.Access,
        Attributes = eqModel.Attributes
    };

    public EquipmentModel CreateDtoToEqModel(CreateEqModelRequestDto eqModel)
    {
        var access = EquipmentModel.EquipmentAccess.User;

        if (eqModel.Osnova)
        {
            access = EquipmentModel.EquipmentAccess.Osnova;
        }
        else if (eqModel.Name.Contains("Ronin", StringComparison.OrdinalIgnoreCase))
        {
            access = EquipmentModel.EquipmentAccess.Ronin;
        }

        return new EquipmentModel
        {
            Name = eqModel.Name,
            Description = eqModel.Description,
            Category = eqModel.Category,
            Attributes = eqModel.Attributes ?? new Dictionary<string, object>(),
            EquipmentItems = new List<EquipmentItem>(),
            Access = access
        };
    }

    public async Task<EqModelResponseDto> CreateEquipmentModel(CreateEqModelRequestDto eqModel)
    {
        if (await _context.EquipmentModels.AnyAsync(eq => EF.Functions.ILike(eq.Name, eqModel.Name)))
            throw new InvalidOperationException("Оборудование с таким названием уже существует");

        var equipmentModel = CreateDtoToEqModel(eqModel);

        _context.EquipmentModels.Add(equipmentModel);
        await _context.SaveChangesAsync();

        return EqModelToResponseDto(equipmentModel);
    }

    public async Task<List<EqModelResponseDto>> GetAllEquipmentModels()
    {
        var eqModels = await _context.EquipmentModels.ToListAsync();

        return eqModels.Select(EqModelToResponseDto).ToList();
    }

    public async Task<EqModelResponseDto?> GetEquipmentModelById(int id)
    {
        var eqModel = await _context.EquipmentModels.FindAsync(id);
        if (eqModel == null)
            return null;
        return EqModelToResponseDto(eqModel);
    }

    public async Task<List<EqModelResponseDto>> GetEquipmentModelByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new List<EqModelResponseDto>();

        var eqModels = await _context.EquipmentModels
            .Where(e => EF.Functions.ILike(e.Name, $"%{name}%"))
            .ToListAsync();

        return eqModels.Select(EqModelToResponseDto).ToList();
    }

    public async Task<List<EqModelResponseDto>> GetEquipmentModelByCategory(EquipmentModel.EquipmentCategory category)
    {
        var eqModels = await _context.EquipmentModels.Where(e => e.Category == category).ToListAsync();
        return eqModels.Select(EqModelToResponseDto).ToList();
    }

    public async Task<List<EqModelResponseDto>> GetAvailableToMe(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new List<EqModelResponseDto>();

        IQueryable<EquipmentModel> query = _context.EquipmentModels;

        switch (user.Role)
        {
            case User.UserRole.Admin:
            case User.UserRole.Ronin:
                break;

            case User.UserRole.Osnova:
                query = query.Where(e =>
                    e.Access == EquipmentModel.EquipmentAccess.User ||
                    e.Access == EquipmentModel.EquipmentAccess.Osnova);
                break;

            case User.UserRole.User:
            default:
                query = query.Where(e => e.Access == EquipmentModel.EquipmentAccess.User);
                break;
        }

        var eqModels = await query.ToListAsync();
        return eqModels.Select(EqModelToResponseDto).ToList();
    }

    public async Task<bool> UpdateEquipmentModel(int id, CreateEqModelRequestDto eqModel)
    {
        var existingModel = await _context.EquipmentModels.FindAsync(id);
        if (existingModel == null)
            return false;

        var nameExists = await _context.EquipmentModels
            .AnyAsync(eq => eq.Id != id && EF.Functions.ILike(eq.Name, eqModel.Name));
        if (nameExists)
            throw new InvalidOperationException("Оборудование с таким названием уже существует");

        var updatedModel = CreateDtoToEqModel(eqModel);

        existingModel.Name = updatedModel.Name;
        existingModel.Description = updatedModel.Description;
        existingModel.Category = updatedModel.Category;
        existingModel.Attributes = updatedModel.Attributes;
        existingModel.Access = updatedModel.Access;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteEquipmentModel(int id)
    {
        var eqModel = await _context.EquipmentModels.FindAsync(id);
        if (eqModel == null) return false;

        _context.EquipmentModels.Remove(eqModel);
        await _context.SaveChangesAsync();
        return true;
    }




    public EqItemResponseDto EqItemToResponseDto(EquipmentItem item)
    {
        return new EqItemResponseDto
        {
            Id = item.Id,
            InventoryNumber = item.InventoryNumber,
            Available = item.Available,
            TypeName = item.EquipmentModel.Name,
            TypeCategory = item.EquipmentModel.Category.ToString()
        };
    }

    public async Task<EqItemResponseDto> CreateEquipmentItem(int equipmentModelId)
    {
        var model = await _context.EquipmentModels
            .FirstOrDefaultAsync(m => m.Id == equipmentModelId);

        if (model == null)
            throw new InvalidOperationException("Модель оборудования не найдена");

        int categoryCode = (int)model.Category;
        var countForType = await _context.EquipmentItems.CountAsync(e => e.EquipmentModelId == equipmentModelId);
        var inventoryNumber = $"{categoryCode}-{equipmentModelId:D3}-{countForType + 1:D2}";

        var newItem = new EquipmentItem
        {
            EquipmentModelId = equipmentModelId,
            InventoryNumber = inventoryNumber,
            Available = true,
            EquipmentModel = model
        };

        _context.EquipmentItems.Add(newItem);
        await _context.SaveChangesAsync();

        return EqItemToResponseDto(newItem);
    }

    public async Task<List<EqItemResponseDto>> GetAllEquipmentItems()
    {
        var items = await _context.EquipmentItems
            .Include(e => e.EquipmentModel)
            .ToListAsync();

        return items.Select(EqItemToResponseDto).ToList();
    }


}