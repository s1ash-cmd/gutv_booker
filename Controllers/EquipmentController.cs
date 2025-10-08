using gutv_booker.Services;
using gutv_booker.Models;
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

        // POST api/equipment/createtype
        [HttpPost("createtype")]
        public async Task<ActionResult<EquipmentType>> CreateEquipmentType([FromBody] EquipmentType equipmentType)
        {
            if (string.IsNullOrEmpty(equipmentType.Name))
            {
                return BadRequest("Название оборудования не может быть пустым");
            }

            if (!Enum.IsDefined(typeof(EquipmentType.EquipmentCategory), equipmentType.Category))
                return BadRequest("Некорректная категория оборудования");

            try
            {
                var eqType = await _equipmentService.CreateEquipmentType(equipmentType.Name, equipmentType.Description,
                    equipmentType.Category, equipmentType.AttributesJson);
                return Ok(eqType);
            }

            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/equipment/get_all_types
        [HttpGet("get_all_types")]
        public async Task<ActionResult<List<EquipmentType>>> GetAllTypes()
        {
            var eqTypes = await _equipmentService.GetAllEquipmentTypes();
            if (!eqTypes.Any())
                return NotFound("Оборудование не найдено");

            return Ok(eqTypes);
        }

        // PUT api/equipment/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateEquipmentType(int id, [FromBody] EquipmentType equipmentType)
        {
            if (id <= 0)
                return BadRequest("ID должен быть больше нуля");

            if (string.IsNullOrWhiteSpace(equipmentType.Name))
                return BadRequest("Название оборудования не может быть пустым");

            if (!Enum.IsDefined(typeof(EquipmentType.EquipmentCategory), equipmentType.Category))
                return BadRequest("Категория оборудования некорректна");

            var success = await _equipmentService.UpdateEquipmentType(id, name: equipmentType.Name, description: equipmentType.Description, category: equipmentType.Category, attributesJson: equipmentType.AttributesJson);

            if (!success) return NotFound($"Оборудование с ID {id} не найдено");
            return Ok("Обновление прошло успешно");
        }

        // DELETE api/equipment/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteEquipmentType(int id)
        {
            if (id <= 0)
                return BadRequest("ID должен быть больше нуля");

            var success = await _equipmentService.DeleteEquipmentType(id);
            if (!success) return NotFound($"Оборудование с ID {id} не найдено");

            return Ok("Удаление прошло успешно");
        }


        // POST api/equipment/createitem
        [HttpPost("createitem")]
        public async Task<ActionResult<EquipmentItem>> CreateEquipmentItem([FromBody] EquipmentItem equipmentItem)
        {
            if (equipmentItem.EquipmentTypeId <= 0)
                return BadRequest("Некорректный EquipmentTypeId");

            try
            {
                var createdItem = await _equipmentService.CreateEquipmentItem(equipmentItem.EquipmentTypeId, equipmentItem.Available);
                return Ok(createdItem);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
