using CinemaBooking.Domain.Models;
using CinemaBooking.Infrastructure.Identity;

namespace CinemaBooking.Application.Notifications;

public interface IPdfTicketService
{
    byte[] GenerateTicket(Booking booking, ApplicationUser user);
}