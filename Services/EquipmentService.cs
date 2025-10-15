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
        Attributes = eqModel.Attributes,
        EquipmentItemsCount = eqModel.EquipmentItems?.Count ?? 0
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

}