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
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [NonAction]
        public int GetIdFromToken()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                              User.FindFirst(ClaimTypes.NameIdentifier);

            return int.Parse(userIdClaim.Value);
        }

        // POST api/booking/create_booking
        [Authorize]
        [HttpPost("create_booking")]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            int userId = GetIdFromToken();

            var booking = await _bookingService.CreateBooking(request, userId);

            if (booking == null)
                return BadRequest();

            return Ok(booking);
        }

        // GET api/booking/get_by_id/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("get_by_id/{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingById(id);
            if (booking == null)
                return NotFound("Бронирование не найдено или не содержит элементов");
            return Ok(booking);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("user/{userId}/bookings")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByUser(int userId)
        {
            var bookings = await _bookingService.GetBookingsByUser(userId);
            return Ok(bookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("equipment/{equipmentItemId}/bookings")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByEquipmentItem(int equipmentItemId)
        {
            var bookings = await _bookingService.GetBookingsByEquipmentItem(equipmentItemId);
            return Ok(bookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}/bookings")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByStatus(Booking.BookingStatus status)
        {
            var bookings = await _bookingService.GetBookingsByStatus(status);
            return Ok(bookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("inventory/{inventoryNumber}/bookings")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByInventoryNumber(string inventoryNumber)
        {
            var bookings = await _bookingService.GetBookingsByInventoryNumber(inventoryNumber);
            return Ok(bookings);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("booking/{bookingId}/approve")]
        public async Task<ActionResult> ApproveBooking(int bookingId)
        {
            var success = await _bookingService.ApproveBooking(bookingId);
            if (!success)
                return NotFound("Бронирование не найдено");
            return Ok();
        }

        //[Authorize(Roles = "Admin")]
        //[HttpPost("booking/{bookingId}/cancel")]
        //public async Task<ActionResult> CancelBooking(int bookingId)
        //{

        //}

        
    }
}