namespace CinemaBooking.Application.Notifications;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "CinemaVerse";
    public bool UseSsl { get; set; } = true;

    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}