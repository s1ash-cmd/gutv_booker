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
        public async Task<ActionResult<EquipmentType>> CreateEquipmentType(
            string name,
            string description,
            EquipmentType.EquipmentCategory category,
            string? attributesJson = null)
        {
            Dictionary<string, object>? attributes = null;

            if (!string.IsNullOrEmpty(attributesJson))
            {
                try
                {
                    attributes = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, object>>(attributesJson);
                }
                catch
                {
                    return BadRequest("Некорректный JSON для атрибутов.");
                }
            }

            var equipmentType = await _equipmentService.CreateEquipmentType(
                name,
                description,
                category,
                attributes
            );

            return Ok(equipmentType);
        }


    }
}
