using CinemaBooking.Domain.DTOs.Users;
using CinemaBooking.Domain.Models;

namespace CinemaBooking.Application.Notifications;

public interface IPdfTicketService
{
    byte[] GenerateTicket(Booking booking, UserInfo user);
}