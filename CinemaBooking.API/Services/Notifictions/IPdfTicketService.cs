using CinemaBooking.Domain;
using CinemaBooking.Infrastructure.Identity;

namespace CinemaBooking.API.Services.Notifications;

public interface IPdfTicketService
{
  
    byte[] GenerateTicket(Booking booking, ApplicationUser user);
}