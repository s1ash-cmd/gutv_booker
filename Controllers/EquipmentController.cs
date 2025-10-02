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
            var eqType = await _equipmentService.CreateEquipmentType(equipmentType.Name, equipmentType.Description, equipmentType.Category, equipmentType.AttributesJson);
            return Ok(eqType);
        }

        // DELETE api/equipment/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult> DeleteEquipmentType(int id)
        {
            var succes = await _equipmentService.DeleteEquipmentType(id);
            if (!succes) return NotFound();
            return Ok();
        }

        // PUT api/equipment/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateEquipmentType(int id, [FromBody] EquipmentType equipmentType)
        {
            var success = await _equipmentService.UpdateEquipmentType(
                id,
                name: equipmentType.Name,
                description: equipmentType.Description,
                category: equipmentType.Category,
                attributesJson: equipmentType.AttributesJson
            );

            if (!success) return NotFound();
            return Ok();
        }

    }
}
