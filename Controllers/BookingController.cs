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
        public async Task<ActionResult<Booking.BookingResponseDto>> CreateBooking(
            [FromBody] Booking.CreateBookingRequestDto request)
        {
            try
            {
                var bookingDto = await _bookingService.CreateBooking(request);
                return Ok(bookingDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}