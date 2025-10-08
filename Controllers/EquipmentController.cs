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

        // POST api/equipment/create_type
        [HttpPost("create_type")]
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

        // GET api/equipment/get_type_by_id/{id}
        [HttpGet("get_type_by_id/{id}")]
        public async Task<ActionResult<EquipmentType>> GetTypeById(int id)
        {
            if (id <= 0)
                return BadRequest("ID должен быть больше нуля");

            var equipmentType = await _equipmentService.GetEquipmentTypeById(id);

            if (equipmentType == null)
                return NotFound($"Оборудование с Id = {id} не найдено");

            return Ok(equipmentType);
        }

        // GET api/equipment/get_type_by_name/{namePart}
        [HttpGet("get_type_by_name/{namePart}")]
        public async Task<ActionResult<List<EquipmentType>>> GetTypeByName(string namePart)
        {
            var eqTypes = await _equipmentService.GetEquipmentTypeByName(namePart);

            if (!eqTypes.Any())
                return NotFound($"Оборудование с названием, содержащим '{namePart}', не найдено");

            return Ok(eqTypes);
        }

        // PUT api/equipment/update_type/{id}
        [HttpPut("update_type/{id}")]
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

        // DELETE api/equipment/delete_type/{id}
        [HttpDelete("delete_type/{id}")]
        public async Task<ActionResult> DeleteEquipmentType(int id)
        {
            if (id <= 0)
                return BadRequest("ID должен быть больше нуля");

            var success = await _equipmentService.DeleteEquipmentType(id);
            if (!success) return NotFound($"Оборудование с ID {id} не найдено");

            return Ok("Удаление прошло успешно");
        }



        // POST api/equipment/create_item
        [HttpPost("create_item")]
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

        // GET api/equipment/get_all_items
        [HttpGet("get_all_items")]
        public async Task<ActionResult<List<EquipmentType>>> GetAllItems()
        {
            var eqItems = await _equipmentService.GetAllEquipmentItems();
            if (!eqItems.Any())
                return NotFound("Оборудование не найдено");

            return Ok(eqItems);
        }

        // GET api/equipment/get_item_by_id/{id}
        [HttpGet("get_item_by_id/{id}")]
        public async Task<ActionResult<EquipmentType>> GetItemById(int id)
        {
            if (id <= 0)
                return BadRequest("ID должен быть больше нуля");

            var equipmentItem = await _equipmentService.GetEquipmentItemById(id);

            if (equipmentItem == null)
                return NotFound($"Оборудование с Id = {id} не найдено");

            return Ok(equipmentItem);
        }

        // DELETE api/equipment/delete_item/{id}
        [HttpDelete("delete_item/{id}")]
        public async Task<ActionResult> DeleteEquipmentItem(int id)
        {
            if (id <= 0)
                return BadRequest("ID должен быть больше нуля");

            var success = await _equipmentService.DeleteEquipmentItem(id);
            if (!success) return NotFound($"Оборудование с ID {id} не найдено");

            return Ok("Удаление прошло успешно");
        }
    }
}
