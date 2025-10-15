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

        // POST api/equipment/create_type
        [Authorize(Roles = "Admin")]
        [HttpPost("create_type")]
        public async Task<ActionResult<EqModelResponseDto>> CreateEquipmentType(
            [FromBody] CreateEqModelRequestDto equipmentType)
        {
            if (string.IsNullOrWhiteSpace(equipmentType.Name))
                return BadRequest("Название оборудования не может быть пустым");

            if (!Enum.IsDefined(typeof(EquipmentModel.EquipmentCategory), equipmentType.Category))
                return BadRequest("Некорректная категория оборудования");

            try
            {
                var eqType = await _equipmentService.CreateEquipmentModel(equipmentType);

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

        // POST api/equipment/get_by_name/{name}
        [HttpGet("get_by_name/{name}")]
        public async Task<ActionResult<List<EqModelResponseDto>>> GetEquipmentModelByName(string name)
        {
            var eqModels = await _equipmentService.GetEquipmentModelByName(name);
            if (!eqModels.Any())
                return NotFound($"Оборудование с именем, содержащим '{name}', не найдено");

            return Ok(eqModels);
        }
    }
}