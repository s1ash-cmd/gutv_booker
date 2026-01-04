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

    public EqModelResponseDto EqModelToResponseDto(EquipmentModel eqModel)
    {
        return new EqModelResponseDto
        {
            Id = eqModel.Id,
            Name = eqModel.Name,
            Description = eqModel.Description,
            Category = eqModel.Category,
            Access = eqModel.Access,
            Attributes = eqModel.Attributes
        };
    }

    public EquipmentModel CreateDtoToEqModel(CreateEqModelRequestDto eqModel)
    {
        if (eqModel == null)
            throw new ArgumentNullException(nameof(eqModel));

        var access = EquipmentModel.EquipmentAccess.User;

        if (eqModel.Name.Contains("Ronin", StringComparison.OrdinalIgnoreCase))
            access = EquipmentModel.EquipmentAccess.Ronin;
        else if (eqModel.Osnova)
            access = EquipmentModel.EquipmentAccess.Osnova;


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
        if (eqModel == null)
            throw new ArgumentNullException(nameof(eqModel));

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

    public async Task<EqModelResponseDto> GetEquipmentModelById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Некорректный ID", nameof(id));

        var eqModel = await _context.EquipmentModels.FindAsync(id);
        if (eqModel == null)
            throw new KeyNotFoundException($"Модель оборудования с ID {id} не найдена");

        return EqModelToResponseDto(eqModel);
    }

    public async Task<List<EqModelResponseDto>> GetEquipmentModelByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название не может быть пустым", nameof(name));

        var eqModels = await _context.EquipmentModels
            .Where(e => EF.Functions.ILike(e.Name, $"%{name}%"))
            .ToListAsync();

        if (!eqModels.Any())
            throw new KeyNotFoundException($"Оборудование с названием '{name}' не найдено");

        return eqModels.Select(EqModelToResponseDto).ToList();
    }

    public async Task<List<EqModelResponseDto>> GetEquipmentModelByCategory(EquipmentModel.EquipmentCategory category)
    {
        var eqModels = await _context.EquipmentModels.Where(e => e.Category == category).ToListAsync();
        if (!eqModels.Any())
            throw new KeyNotFoundException($"Оборудование категории {category} не найдено");

        return eqModels.Select(EqModelToResponseDto).ToList();
    }

    public async Task<List<EqModelResponseDto>> GetAvailableToMe(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден");

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
                query = query.Where(e => e.Access == EquipmentModel.EquipmentAccess.User);
                break;

            default:
                throw new UnauthorizedAccessException("Неизвестная роль пользователя");
        }

        var eqModels = await query.ToListAsync();
        return eqModels.Select(EqModelToResponseDto).ToList();
    }

    public async Task UpdateEquipmentModel(int id, CreateEqModelRequestDto eqModel)
    {
        if (id <= 0)
            throw new ArgumentException("ID должен быть положительным", nameof(id));
        if (eqModel == null)
            throw new ArgumentNullException(nameof(eqModel));

        var existingModel = await _context.EquipmentModels.FindAsync(id);
        if (existingModel == null)
            throw new KeyNotFoundException($"Модель оборудования с ID {id} не найдена");

        var nameExists = await _context.EquipmentModels
            .AnyAsync(eq => eq.Id != id && EF.Functions.ILike(eq.Name, eqModel.Name.Trim()));
        if (nameExists)
            throw new InvalidOperationException("Оборудование с таким названием уже существует");

        var updatedModel = CreateDtoToEqModel(eqModel);

        existingModel.Name = updatedModel.Name;
        existingModel.Description = updatedModel.Description;
        existingModel.Category = updatedModel.Category;
        existingModel.Attributes = updatedModel.Attributes;
        existingModel.Access = updatedModel.Access;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteEquipmentModel(int id)
    {
        var eqModel = await _context.EquipmentModels.FindAsync(id);
        if (eqModel == null)
            throw new KeyNotFoundException($"Модель оборудования с ID {id} не найдена");

        _context.EquipmentModels.Remove(eqModel);
        await _context.SaveChangesAsync();
    }

    public EqItemResponseDto EqItemToResponseDto(EquipmentItem item)
    {
        return new EqItemResponseDto
        {
            Id = item.Id,
            InventoryNumber = item.InventoryNumber,
            Available = item.Available,
            ModelName = item.EquipmentModel.Name,
            ModelCategory = item.EquipmentModel.Category.ToString()
        };
    }

    public async Task<EqItemResponseDto> CreateEquipmentItem(int equipmentModelId)
    {
        var model = await _context.EquipmentModels.FirstOrDefaultAsync(m => m.Id == equipmentModelId);
        if (model == null)
            throw new KeyNotFoundException("Модель оборудования не найдена");

        var categoryCode = (int)model.Category;

        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM \"EquipmentModels\" WHERE \"Id\" = {0} FOR UPDATE",
                equipmentModelId);

            var lastItem = await _context.EquipmentItems
                .Where(e => e.EquipmentModelId == equipmentModelId)
                .OrderByDescending(e => e.Id)
                .Select(e => e.InventoryNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastItem != null)
            {
                var parts = lastItem.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            var inventoryNumber = $"{categoryCode}-{equipmentModelId:D3}-{nextNumber:D2}";

            var newItem = new EquipmentItem
            {
                EquipmentModelId = equipmentModelId,
                InventoryNumber = inventoryNumber,
                Available = true,
                EquipmentModel = model
            };

            _context.EquipmentItems.Add(newItem);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return EqItemToResponseDto(newItem);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<EqModelWithItemsDto>> GetModelsWithItems()
    {
        var eqModels = await _context.EquipmentModels
            .Include(m => m.EquipmentItems)
            .ToListAsync();

        var result = eqModels.Select(m => new EqModelWithItemsDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            Category = m.Category,
            Access = m.Access,
            Attributes = m.Attributes,
            Items = m.EquipmentItems.Select(EqItemToResponseDto).ToList()
        }).ToList();

        return result;
    }

    public async Task<List<EqItemResponseDto>> GetAllEquipmentItems()
    {
        var items = await _context.EquipmentItems
            .Include(e => e.EquipmentModel)
            .ToListAsync();

        return items.Select(EqItemToResponseDto).ToList();
    }

    public async Task<EqItemResponseDto> GetEquipmentItemById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Некорректный ID", nameof(id));

        var item = await _context.EquipmentItems
            .Include(e => e.EquipmentModel)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (item == null)
            throw new KeyNotFoundException($"Экземпляр оборудования с ID {id} не найден");

        return EqItemToResponseDto(item);
    }

    public async Task<List<EqItemResponseDto>> GetEquipmentItemsByModel(int equipmentModelId)
    {
        var exists = await _context.EquipmentModels.AnyAsync(m => m.Id == equipmentModelId);
        if (!exists)
            throw new KeyNotFoundException($"Модель оборудования с ID {equipmentModelId} не найдена");

        var items = await _context.EquipmentItems
            .Include(e => e.EquipmentModel)
            .Where(e => e.EquipmentModelId == equipmentModelId)
            .ToListAsync();

        if (!items.Any())
            throw new KeyNotFoundException($"Нет экземпляров для модели {equipmentModelId}");

        return items.Select(EqItemToResponseDto).ToList();
    }

    public async Task DeleteEquipmentItem(int id)
    {
        var equipmentItem = await _context.EquipmentItems.FindAsync(id);
        if (equipmentItem == null)
            throw new KeyNotFoundException($"Экземпляр оборудования с ID {id} не найден");

        _context.EquipmentItems.Remove(equipmentItem);
        await _context.SaveChangesAsync();
    }

    public async Task<List<EqItemResponseDto>> GetAvailableEquipmentItemsByModel(
        int equipmentModelId,
        DateTime start,
        DateTime end)
    {
        if (equipmentModelId <= 0)
            throw new ArgumentException("Некорректный ID модели", nameof(equipmentModelId));

        if (start >= end)
            throw new ArgumentException("Дата начала должна быть раньше даты окончания");

        var exists = await _context.EquipmentModels.AnyAsync(m => m.Id == equipmentModelId);
        if (!exists)
            throw new KeyNotFoundException($"Модель оборудования с ID {equipmentModelId} не найдена");

        var items = await _context.EquipmentItems
            .AsNoTracking()
            .Include(e => e.EquipmentModel)
            .Where(e => e.EquipmentModelId == equipmentModelId)
            .Where(e => e.Available)
            .Where(e => !e.BookingItems.Any(bi =>
                (bi.Booking.Status == Booking.BookingStatus.Pending ||
                 bi.Booking.Status == Booking.BookingStatus.Approved) &&
                start < bi.EndDate && end > bi.StartDate
            ))
            .ToListAsync();

        return items.Select(EqItemToResponseDto).ToList();
    }

    public async Task ToggleAvailability(int id)
    {
        var equipmentItem = await _context.EquipmentItems.FindAsync(id);
        if (equipmentItem == null)
            throw new KeyNotFoundException($"Экземпляр оборудования с ID {id} не найден");

        equipmentItem.Available = !equipmentItem.Available;
        await _context.SaveChangesAsync();
    }
}