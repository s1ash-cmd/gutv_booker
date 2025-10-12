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
        public async Task<ActionResult<EquipmentType>> CreateEquipmentType([FromBody] EquipmentType equipmentType)
        {
            if (string.IsNullOrEmpty(equipmentType.Name))
                return BadRequest("Название оборудования не может быть пустым");

            if (!Enum.IsDefined(typeof(EquipmentType.EquipmentCategory), equipmentType.Category))
                return BadRequest("Некорректная категория оборудования");

            try
            {
                var eqType = await _equipmentService.CreateEquipmentType(
                    equipmentType.Name,
                    equipmentType.Description,
                    equipmentType.Category,
                    equipmentType.Osnova,
                    equipmentType.AttributesJson
                );
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
        [Authorize(Roles = "Admin")]
        [HttpGet("get_type_by_id/{id}")]
        public async Task<ActionResult<EquipmentType>> GetTypeById(int id)
        {
            if (id <= 0)
                return BadRequest("ID должен быть больше нуля");

            var eqType = await _equipmentService.GetEquipmentTypeById(id);
            if (eqType == null)
                return NotFound($"Оборудование с Id = {id} не найдено");

            return Ok(eqType);
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

        // GET api/equipment/get_type_by_category/category
        [HttpGet("get_type_by_category/{category}")]
        public async Task<ActionResult<List<EquipmentType>>> GetEquipmentTypeByCategory(
            EquipmentType.EquipmentCategory category)
        {
            var eqTypes = await _equipmentService.GetEquipmentTypeByCategory(category);

            if (!eqTypes.Any())
                return NotFound($"Оборудование категории '{category}' не найдено");

            return Ok(eqTypes);
        }

        // PUT api/equipment/update_type/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("update_type/{id}")]
        public async Task<ActionResult> UpdateEquipmentType(int id, [FromBody] EquipmentType equipmentType)
        {
            if (id <= 0) return BadRequest("ID должен быть больше нуля");
            if (string.IsNullOrWhiteSpace(equipmentType.Name))
                return BadRequest("Название оборудования не может быть пустым");
            if (!Enum.IsDefined(typeof(EquipmentType.EquipmentCategory), equipmentType.Category))
                return BadRequest("Категория оборудования некорректна");

            var success = await _equipmentService.UpdateEquipmentType(
                id,
                name: equipmentType.Name,
                description: equipmentType.Description,
                category: equipmentType.Category,
                attributesJson: equipmentType.AttributesJson
            );

            if (!success) return NotFound($"Оборудование с ID {id} не найдено");
            return Ok("Обновление прошло успешно");
        }

        // DELETE api/equipment/delete_type/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_type/{id}")]
        public async Task<ActionResult> DeleteEquipmentType(int id)
        {
            if (id <= 0) return BadRequest("ID должен быть больше нуля");

            var success = await _equipmentService.DeleteEquipmentType(id);
            if (!success) return NotFound($"Оборудование с ID {id} не найдено");

            return Ok("Удаление прошло успешно");
        }


        // POST api/equipment/create_item
        [Authorize(Roles = "Admin")]
        [HttpPost("create_item")]
        public async Task<ActionResult<EquipmentItemDto>> CreateEquipmentItem([FromBody] EquipmentItemDto itemDto)
        {
            if (itemDto.EquipmentTypeId <= 0)
                return BadRequest("Некорректный EquipmentTypeId");

            try
            {
                var dto = await _equipmentService.CreateEquipmentItemDto(itemDto.EquipmentTypeId, itemDto.Available);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/equipment/get_all_items
        [HttpGet("get_all_items")]
        public async Task<ActionResult<List<EquipmentItemDto>>> GetAllItems()
        {
            var items = await _equipmentService.GetAllEquipmentItems();
            if (!items.Any()) return NotFound("Оборудование не найдено");

            var dtos = items.Select(i => new EquipmentItemDto
            {
                Id = i.Id,
                EquipmentTypeId = i.EquipmentTypeId,
                InventoryNumber = i.InventoryNumber,
                Available = i.Available
            }).ToList();

            return Ok(dtos);
        }

        // GET api/equipment/get_item_by_id/{id}
        [HttpGet("get_item_by_id/{id}")]
        public async Task<ActionResult<EquipmentItemDto>> GetItemById(int id)
        {
            if (id <= 0) return BadRequest("ID должен быть больше нуля");

            var item = await _equipmentService.GetEquipmentItemById(id);
            if (item == null) return NotFound($"Оборудование с Id = {id} не найдено");

            var dto = new EquipmentItemDto
            {
                Id = item.Id,
                EquipmentTypeId = item.EquipmentTypeId,
                InventoryNumber = item.InventoryNumber,
                Available = item.Available
            };

            return Ok(dto);
        }

        // GET api/equipment/get_item_by_type/{typeId}
        [HttpGet("get_item_by_type/{typeId}")]
        public async Task<ActionResult<List<EquipmentItemDto>>> GetItemsByType(int typeId)
        {
            if (typeId <= 0) return BadRequest("ID должен быть больше нуля");

            var items = await _equipmentService.GetEquipmentItemsByType(typeId);
            if (!items.Any()) return NotFound("Оборудование не найдено");

            var dtos = items.Select(i => new EquipmentItemDto
            {
                Id = i.Id,
                EquipmentTypeId = i.EquipmentTypeId,
                InventoryNumber = i.InventoryNumber,
                Available = i.Available
            }).ToList();

            return Ok(dtos);
        }

        // DELETE api/equipment/delete_item/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_item/{id}")]
        public async Task<ActionResult> DeleteEquipmentItem(int id)
        {
            if (id <= 0) return BadRequest("ID должен быть больше нуля");

            var success = await _equipmentService.DeleteEquipmentItem(id);
            if (!success) return NotFound($"Оборудование с ID {id} не найдено");

            return Ok("Удаление прошло успешно");
        }
    }
}