using CinemaBooking.API.DTOs.Bookings;
using CinemaBooking.API.DTOs.Common;

namespace CinemaBooking.API.Services;

public interface IBookingService
{
    PagedResult<BookingDto> GetAll(BookingSearchRequest request);
    BookingDto? GetById(long id);
    (BookingDto? dto, string? errorMessage, int statusCode) Create(CreateBookingRequest request);
    (bool success, string? errorMessage) Cancel(long id);
}