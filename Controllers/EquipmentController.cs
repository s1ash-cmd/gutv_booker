using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using gutv_booker.Models;
using gutv_booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gutv_booker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EquipmentController : ControllerBase
{
    private readonly EquipmentService _equipmentService;

    public EquipmentController(EquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [NonAction]
    public int GetIdFromToken()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                          User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Не удалось получить идентификатор пользователя из токена");

        return int.Parse(userIdClaim.Value);
    }

    // POST api/equipment/create_model
    [Authorize(Roles = "Admin")]
    [HttpPost("create_model")]
    public async Task<ActionResult<EqModelResponseDto>> CreateEquipmentModel(
        [FromBody] CreateEqModelRequestDto equipmentModel)
    {
        try
        {
            var eqModel = await _equipmentService.CreateEquipmentModel(equipmentModel);
            return Ok(eqModel);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET api/equipment/get_models_with_items
    [HttpGet("get_models_with_items")]
    public async Task<ActionResult<List<EqModelWithItemsDto>>> GetModelsWithItems()
    {
        var modelsWithItems = await _equipmentService.GetModelsWithItems();
        return Ok(modelsWithItems);
    }

    // GET api/equipment/get_all_models
    [HttpGet("get_all_models")]
    public async Task<ActionResult<List<EqModelResponseDto>>> GetAllEquipmentModels()
    {
        var eqModels = await _equipmentService.GetAllEquipmentModels();
        return Ok(eqModels);
    }

    // GET api/equipment/get_model_by_id/{id}
    [HttpGet("get_model_by_id/{id}")]
    public async Task<ActionResult<EqModelResponseDto>> GetEquipmentModelById(int id)
    {
        try
        {
            var eqModel = await _equipmentService.GetEquipmentModelById(id);
            return Ok(eqModel);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET api/equipment/get_model_by_name/{name}
    [HttpGet("get_model_by_name/{name}")]
    public async Task<ActionResult<List<EqModelResponseDto>>> GetEquipmentModelByName(string name)
    {
        try
        {
            var eqModels = await _equipmentService.GetEquipmentModelByName(name);
            return Ok(eqModels);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET api/equipment/get_model_by_category/{category}
    [HttpGet("get_model_by_category/{category}")]
    public async Task<ActionResult<List<EqModelResponseDto>>> GetEquipmentModelByCategory(
        EquipmentModel.EquipmentCategory category)
    {
        try
        {
            var eqModels = await _equipmentService.GetEquipmentModelByCategory(category);
            return Ok(eqModels);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET api/equipment/available_models_to_me
    [Authorize]
    [HttpGet("available_models_to_me")]
    public async Task<ActionResult<List<EqModelResponseDto>>> GetAvailableToMe()
    {
        try
        {
            var userId = GetIdFromToken();
            var eqModels = await _equipmentService.GetAvailableToMe(userId);
            return Ok(eqModels);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    // GET api/equipment/get_available_items
    [HttpGet("get_available_items_by_model")]
    public async Task<ActionResult<List<EqItemResponseDto>>> GetAvailableItemsByModel(
        [FromQuery] int modelId,
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        try
        {
            var items = await _equipmentService.GetAvailableEquipmentItemsByModel(modelId, start, end);
            return Ok(items);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT api/equipment/update_model/{id}
    [Authorize(Roles = "Admin")]
    [HttpPut("update_model/{id}")]
    public async Task<ActionResult> UpdateEquipmentModel(int id, [FromBody] CreateEqModelRequestDto eqModel)
    {
        try
        {
            await _equipmentService.UpdateEquipmentModel(id, eqModel);
            return Ok("Оборудование успешно обновлено");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE api/equipment/delete_model/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete_model/{id}")]
    public async Task<ActionResult> DeleteEquipmentModel(int id)
    {
        try
        {
            await _equipmentService.DeleteEquipmentModel(id);
            return Ok("Модель оборудования успешно удалена");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST api/equipment/create_item?equipmentModelId=3
    [Authorize(Roles = "Admin")]
    [HttpPost("create_item")]
    public async Task<ActionResult<EqItemResponseDto>> CreateEquipmentItem([FromQuery] int equipmentModelId)
    {
        try
        {
            var item = await _equipmentService.CreateEquipmentItem(equipmentModelId);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET api/equipment/get_all_items
    [HttpGet("get_all_items")]
    public async Task<ActionResult<List<EqItemResponseDto>>> GetAllEquipmentItems()
    {
        var items = await _equipmentService.GetAllEquipmentItems();
        return Ok(items);
    }

    // GET api/equipment/get_item_by_id/{id}
    [HttpGet("get_item_by_id/{id}")]
    public async Task<ActionResult<EqItemResponseDto>> GetEquipmentItemById(int id)
    {
        try
        {
            var item = await _equipmentService.GetEquipmentItemById(id);
            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET api/equipment/get_items_by_model/{modelId}
    [HttpGet("get_items_by_model/{modelId}")]
    public async Task<ActionResult<List<EqItemResponseDto>>> GetEquipmentItemsByModel(int modelId)
    {
        try
        {
            var items = await _equipmentService.GetEquipmentItemsByModel(modelId);
            return Ok(items);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PATCH api/equipment/toggle_availability/{id}
    [Authorize(Roles = "Admin")]
    [HttpPatch("toggle_availability/{id}")]
    public async Task<ActionResult> ToggleEquipmentItemAvailability(int id)
    {
        try
        {
            await _equipmentService.ToggleAvailability(id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE api/equipment/delete_item/{id}
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete_item/{id}")]
    public async Task<ActionResult> DeleteEquipmentItem(int id)
    {
        try
        {
            await _equipmentService.DeleteEquipmentItem(id);
            return Ok("Экземпляр оборудования успешно удалён");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}