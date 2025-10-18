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

        // POST api/equipment/create_type
        [Authorize(Roles = "Admin")]
        [HttpPost("create_type")]
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

        // POST api/equipment/get_all
        [HttpGet("get_all")]
        public async Task<ActionResult<List<EqModelResponseDto>>> GetAllEquipmentModels()
        {
            var eqModels = await _equipmentService.GetAllEquipmentModels();
            return Ok(eqModels);
        }

        // POST api/equipment/get_by_id/{id}
        [HttpGet("get_by_id/{id}")]
        public async Task<ActionResult<EqModelResponseDto>> GetEquipmentModelById(int id)
        {
            var eqModel = await _equipmentService.GetEquipmentModelById(id);
            if (eqModel == null)
                return NotFound($"Оборудование с Id {id} не найдено");

            return Ok(eqModel);
        }

        // GET api/equipment/get_by_name/{name}
        [HttpGet("get_by_name/{name}")]
        public async Task<ActionResult<List<EqModelResponseDto>>> GetEquipmentModelByName(string name)
        {
            var eqModels = await _equipmentService.GetEquipmentModelByName(name);
            if (!eqModels.Any())
                return NotFound($"Оборудование с названием '{name}' не найдено");

            return Ok(eqModels);
        }

        // GET api/equipment/get_by_category/{category}
        [HttpGet("get_by_category/{category}")]
        public async Task<ActionResult<List<EqModelResponseDto>>> GetEquipmentModelByCategory(
            EquipmentModel.EquipmentCategory category)
        {
            var eqModels = await _equipmentService.GetEquipmentModelByCategory(category);
            if (!eqModels.Any())
                return NotFound($"Оборудование из этой категории не найдено");

            return Ok(eqModels);
        }

        // GET api/equipment/available_to_me
        [Authorize]
        [HttpGet("available_to_me")]
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

        // PUT api/equipment/update/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
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
    }
}