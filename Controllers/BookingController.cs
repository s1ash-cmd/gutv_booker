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
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                  User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return Unauthorized("Пользователь не авторизован");

                var userId = int.Parse(userIdClaim.Value);

                var bookingDto = await _bookingService.CreateBooking(request, userId);
                return Ok(bookingDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/booking/{id}
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBookingById(int id)
        {
            try
            {
                var bookingDto = await _bookingService.GetBookingById(id);
                return Ok(bookingDto);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // GET api/booking/user/{userId}
        [Authorize(Roles = "Admin")]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByUser(int userId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByUser(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/booking/user/me
        [Authorize]
        [HttpGet("user/me")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetMyBookings()
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                  User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return Unauthorized("Пользователь не авторизован");

                var userId = int.Parse(userIdClaim.Value);

                var bookings = await _bookingService.GetBookingsByUser(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/booking/equipment/{equipmentItemId}
        [Authorize(Roles = "Admin")]
        [HttpGet("equipment/{equipmentItemId}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByEquipmentItem(int equipmentItemId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByEquipmentItem(equipmentItemId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/booking/status/{status}
        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByStatus(string status)
        {
            try
            {
                if (!Enum.TryParse<Booking.BookingStatus>(status, true, out var bookingStatus))
                    return BadRequest(new { error = "Неверный статус брони" });

                var bookings = await _bookingService.GetBookingsByStatus(bookingStatus);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET api/booking/invnumber/{invNumber}
        [Authorize(Roles = "Admin")]
        [HttpGet("invnumber/{invNumber}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByInvNumber(string invNumber)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByInventoryNumber(invNumber);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PATCH api/booking/approve/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("approve/{id}")]
        public async Task<ActionResult> ApproveBooking(int id)
        {
            try
            {
                await _bookingService.ApproveBooking(id);
                return Ok(new { message = "Бронь подтверждена" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PATCH api/booking/complete/{id}
        [Authorize(Roles = "Admin")]
        [HttpPatch("complete/{id}")]
        public async Task<ActionResult> CompleteBooking(int id)
        {
            try
            {
                await _bookingService.CompleteBooking(id);
                return Ok(new { message = "Бронь завершена" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE api/booking/cancel/{id}
        [Authorize]
        [HttpDelete("cancel/{id}")]
        public async Task<ActionResult> CancelBooking(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                  User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return Unauthorized("Пользователь не авторизован");

                var userId = int.Parse(userIdClaim.Value);
                var isAdmin = User.IsInRole("Admin");

                await _bookingService.CancelBooking(id, userId, isAdmin);
                return Ok(new { message = "Бронь отменена и удалена" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}