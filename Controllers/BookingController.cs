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

            return Ok(booking);
        }
    }
}