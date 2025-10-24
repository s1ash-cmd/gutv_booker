using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using gutv_booker.Models;
using gutv_booker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gutv_booker.Controllers;

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

        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Пользователь не авторизован");

        return int.Parse(userIdClaim.Value);
    }

    // POST api/booking/create_booking
    [Authorize]
    [HttpPost("create_booking")]
    public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] CreateBookingRequestDto request)
    {
        try
        {
            var userId = GetIdFromToken();
            var booking = await _bookingService.CreateBooking(request, userId);
            return Ok(booking);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET api/booking/get_by_id/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet("get_by_id/{id}")]
    public async Task<ActionResult<BookingResponseDto>> GetBookingById(int id)
    {
        try
        {
            var booking = await _bookingService.GetBookingById(id);
            return Ok(booking);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET api/booking/get_by_user/{userId}
    [Authorize(Roles = "Admin")]
    [HttpGet("get_by_user/{userId}")]
    public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByUser(int userId)
    {
        try
        {
            var bookings = await _bookingService.GetBookingsByUser(userId);
            return Ok(bookings);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET api/booking/get_by_item/{equipmentItemId}
    [Authorize(Roles = "Admin")]
    [HttpGet("get_by_item/{equipmentItemId}")]
    public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByEquipmentItem(int equipmentItemId)
    {
        try
        {
            var bookings = await _bookingService.GetBookingsByEquipmentItem(equipmentItemId);
            return Ok(bookings);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET api/booking/get_by_status/{status}
    [Authorize(Roles = "Admin")]
    [HttpGet("get_by_status/{status}")]
    public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByStatus(Booking.BookingStatus status)
    {
        try
        {
            var bookings = await _bookingService.GetBookingsByStatus(status);
            return Ok(bookings);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // GET api/booking/get_by_invnum/{inventoryNumber}
    [Authorize(Roles = "Admin")]
    [HttpGet("get_by_invnum/{inventoryNumber}")]
    public async Task<ActionResult<List<BookingResponseDto>>> GetBookingsByInventoryNumber(string inventoryNumber)
    {
        try
        {
            var bookings = await _bookingService.GetBookingsByInventoryNumber(inventoryNumber);
            return Ok(bookings);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PATCH api/booking/approve/{bookingId}
    [Authorize(Roles = "Admin")]
    [HttpPatch("approve/{bookingId}")]
    public async Task<ActionResult> ApproveBooking(int bookingId, [FromBody] string adminComment)
    {
        try
        {
            await _bookingService.ApproveBooking(bookingId, adminComment);
            return Ok(new { message = "Бронирование успешно одобрено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PATCH api/booking/reject/{bookingId}
    [Authorize(Roles = "Admin")]
    [HttpPatch("reject/{bookingId}")]
    public async Task<ActionResult> RejectBooking(int bookingId, [FromBody] string adminComment)
    {
        try
        {
            await _bookingService.CancelBooking(bookingId, 0, true, adminComment);
            return Ok(new { message = "Бронирование отклонено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PATCH api/booking/complete/{id}
    [Authorize(Roles = "Admin")]
    [HttpPatch("complete/{id}")]
    public async Task<ActionResult> CompleteBooking(int id)
    {
        try
        {
            var success = await _bookingService.CompleteBooking(id);
            if (!success)
                return NotFound(new { error = $"Бронь с Id {id} не найдена" });

            return Ok(new { message = "Бронь завершена" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PATCH api/booking/cancel/{id}
    [Authorize]
    [HttpPatch("cancel/{id}")]
    public async Task<ActionResult> CancelBooking(int id, [FromBody] string? adminComment = null)
    {
        try
        {
            var userId = GetIdFromToken();
            var isAdmin = User.IsInRole("Admin");

            await _bookingService.CancelBooking(id, userId, isAdmin, adminComment);
            return Ok(new { message = "Бронирование отменено" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
