using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using gutv_booker.Services;
using gutv_booker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gutv_booker.Controllers
{
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

            return int.Parse(userIdClaim.Value);
        }

        // POST api/equipment/create_model
        [Authorize(Roles = "Admin")]
        [HttpPost("create_model")]
        public async Task<ActionResult<EqModelResponseDto>> CreateEquipmentType(
            [FromBody] CreateEqModelRequestDto equipmentModel)
        {
            if (string.IsNullOrWhiteSpace(equipmentModel.Name))
                return BadRequest("Название оборудования не может быть пустым");

            if (!Enum.IsDefined(typeof(EquipmentModel.EquipmentCategory), equipmentModel.Category))
                return BadRequest("Некорректная категория оборудования");

            try
            {
                var eqType = await _equipmentService.CreateEquipmentModel(equipmentModel);

                return Ok(eqType);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/equipment/get_all_models
        [HttpGet("get_all_models")]
        public async Task<ActionResult<List<EqModelResponseDto>>> GetAllEquipmentModels()
        {
            var eqModels = await _equipmentService.GetAllEquipmentModels();
            return Ok(eqModels);
        }

        // POST api/equipment/get_model_by_id/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("get_model_by_id/{id}")]
        public async Task<ActionResult<EqModelResponseDto>> GetEquipmentModelById(int id)
        {
            var eqModel = await _equipmentService.GetEquipmentModelById(id);
            if (eqModel == null)
                return NotFound($"Оборудование с Id {id} не найдено");

            return Ok(eqModel);
        }

        // GET api/equipment/get_model_by_name/{name}
        [HttpGet("get_model_by_name/{name}")]
        public async Task<ActionResult<List<EqModelResponseDto>>> GetEquipmentModelByName(string name)
        {
            var eqModels = await _equipmentService.GetEquipmentModelByName(name);
            if (!eqModels.Any())
                return NotFound($"Оборудование с названием '{name}' не найдено");

            return Ok(eqModels);
        }

        // GET api/equipment/get_model_by_category/{category}
        [HttpGet("get_model_by_category/{category}")]
        public async Task<ActionResult<List<EqModelResponseDto>>> GetEquipmentModelByCategory(
            EquipmentModel.EquipmentCategory category)
        {
            var eqModels = await _equipmentService.GetEquipmentModelByCategory(category);
            if (!eqModels.Any())
                return NotFound($"Оборудование из этой категории не найдено");

            return Ok(eqModels);
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        // PUT api/equipment/update_model/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("update_model/{id}")]
        public async Task<ActionResult> UpdateEquipmentModel(int id, [FromBody] CreateEqModelRequestDto eqModel)
        {
            if (string.IsNullOrWhiteSpace(eqModel.Name))
                return BadRequest("Название оборудования не может быть пустым");

            if (!Enum.IsDefined(typeof(EquipmentModel.EquipmentCategory), eqModel.Category))
                return BadRequest("Некорректная категория оборудования");

            try
            {
                var success = await _equipmentService.UpdateEquipmentModel(id, eqModel);
                if (!success)
                    return NotFound($"Оборудование с Id {id} не найдено");

                return Ok("Оборудование успешно обновлено");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/equipment/delete_model/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_model/{id}")]
        public async Task<ActionResult> DeleteEquipmentModel(int id)
        {
            if (id <= 0) return BadRequest("ID должен быть больше нуля");

            var success = await _equipmentService.DeleteEquipmentModel(id);
            if (!success) return NotFound($"Оборудование с ID {id} не найдено");

            return Ok("Удаление прошло успешно");
        }


        // POST api/equipment/create_item
        [Authorize(Roles = "Admin")]
        [HttpPost("create_item")]
        public async Task<ActionResult<EqItemResponseDto>> CreateEquipmentItem(int equipmentModelId)
        {
            try
            {
                var item = await _equipmentService.CreateEquipmentItem(equipmentModelId);
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/equipment
        [HttpGet("get_all_items")]
        public async Task<ActionResult<List<EqItemResponseDto>>> GetAllEquipmentItems()
        {
            var items = await _equipmentService.GetAllEquipmentItems();
            return Ok(items);
        }
    }
}