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

        // POST api/booking/create_booking
        [Authorize]
        [HttpPost("create_booking")]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                              User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("Пользователь не авторизован");

            var userId = int.Parse(userIdClaim.Value);

            var (success, booking, warnings) = await _bookingService.CreateBooking(request, userId);

            if (!success || booking == null)
                return BadRequest(new { error = warnings?.FirstOrDefault() ?? "Не удалось создать бронь" });

            return Ok(booking);
        }

        // GET api/booking/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById(int id)
        {
            var (success, booking) = await _bookingService.GetBookingById(id);

            if (!success || booking == null)
                return NotFound(new { error = $"Бронь с Id {id} не найдена" });

            return Ok(booking);
        }

        // GET api/booking/user/{userId}
        [Authorize(Roles = "Admin")]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByUser(int userId)
        {
            var bookings = await _bookingService.GetBookingsByUser(userId);
            return Ok(bookings);
        }

        // GET api/booking/user/me
        [Authorize]
        [HttpGet("user/me")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetMyBookings()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                              User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("Пользователь не авторизован");

            var userId = int.Parse(userIdClaim.Value);
            var bookings = await _bookingService.GetBookingsByUser(userId);
            return Ok(bookings);
        }

        // GET api/booking/equipment/{equipmentItemId}
        [Authorize(Roles = "Admin")]
        [HttpGet("equipment/{equipmentItemId}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByEquipmentItem(int equipmentItemId)
        {
            var bookings = await _bookingService.GetBookingsByEquipmentItem(equipmentItemId);
            return Ok(bookings);
        }

        // GET api/booking/status/{status}
        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByStatus(string status)
        {
            if (!Enum.TryParse<Booking.BookingStatus>(status, true, out var bookingStatus))
                return BadRequest(new { error = "Неверный статус брони" });

            var bookings = await _bookingService.GetBookingsByStatus(bookingStatus);
            return Ok(bookings);
        }

        // GET api/booking/invnumber/{invNumber}
        [Authorize(Roles = "Admin")]
        [HttpGet("invnumber/{invNumber}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByInvNumber(string invNumber)
        {
            var bookings = await _bookingService.GetBookingsByInventoryNumber(invNumber);
            return Ok(bookings);
        }

        // PATCH api/booking/approve/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("approve/{id}")]
        public async Task<ActionResult> ApproveBooking(int id)
        {
            var success = await _bookingService.ApproveBooking(id);
            if (!success) return NotFound(new { error = $"Бронь с Id {id} не найдена" });

            return Ok(new { message = "Бронь подтверждена" });
        }

        // PATCH api/booking/complete/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("complete/{id}")]
        public async Task<ActionResult> CompleteBooking(int id)
        {
            var success = await _bookingService.CompleteBooking(id);
            if (!success) return NotFound(new { error = $"Бронь с Id {id} не найдена" });

            return Ok(new { message = "Бронь завершена" });
        }

        // DELETE api/booking/cancel/{id}
        [Authorize]
        [HttpDelete("cancel/{id}")]
        public async Task<ActionResult> CancelBooking(int id)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                              User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("Пользователь не авторизован");

            var userId = int.Parse(userIdClaim.Value);
            var isAdmin = User.IsInRole("Admin");

            var success = await _bookingService.CancelBooking(id, userId, isAdmin);
            if (!success)
                return Forbid("Вы не можете удалить чужую бронь или бронь не найдена");

            return Ok(new { message = "Бронь отменена и удалена" });
        }
    }
}