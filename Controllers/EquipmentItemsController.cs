using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gutv_booker.Data;
using gutv_booker.Models;

namespace gutv_booker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EquipmentItemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EquipmentItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipmentItem>>> GetEquipmentItems()
        {
            return await _context.EquipmentItems.ToListAsync();
        }

        // GET: api/EquipmentItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EquipmentItem>> GetEquipmentItem(int id)
        {
            var equipmentItem = await _context.EquipmentItems.FindAsync(id);

            if (equipmentItem == null)
            {
                return NotFound();
            }

            return equipmentItem;
        }

        // PUT: api/EquipmentItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipmentItem(int id, EquipmentItem equipmentItem)
        {
            if (id != equipmentItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(equipmentItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipmentItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/EquipmentItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EquipmentItem>> PostEquipmentItem(EquipmentItem equipmentItem)
        {
            _context.EquipmentItems.Add(equipmentItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEquipmentItem", new { id = equipmentItem.Id }, equipmentItem);
        }

        // DELETE: api/EquipmentItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipmentItem(int id)
        {
            var equipmentItem = await _context.EquipmentItems.FindAsync(id);
            if (equipmentItem == null)
            {
                return NotFound();
            }

            _context.EquipmentItems.Remove(equipmentItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EquipmentItemExists(int id)
        {
            return _context.EquipmentItems.Any(e => e.Id == id);
        }
    }
}
