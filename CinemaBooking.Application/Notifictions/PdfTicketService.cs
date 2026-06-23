using CinemaBooking.Application.Config;
using CinemaBooking.Domain.DTOs.Users;
using CinemaBooking.Domain.Models;
using Microsoft.Extensions.Options;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CinemaBooking.Application.Notifications;

public class PdfTicketService : IPdfTicketService
{
    private readonly string _frontendUrl;

    private static readonly TimeZoneInfo BelgradeTz = GetBelgradeTz();

    private static TimeZoneInfo GetBelgradeTz()
    {
        foreach (var id in new[] { "Europe/Belgrade", "Central European Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch { /* probaj sledeci */ }
        }

        return TimeZoneInfo.Utc;
    }

    public PdfTicketService(IOptions<AppSettings> appSettings)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _frontendUrl = appSettings.Value.FrontendUrl.TrimEnd('/');
    }

    public byte[] GenerateTicket(Booking booking, UserInfo user)
    {
        var qrBytes = GenerateQrCode(booking, _frontendUrl);
        return GeneratePdf(booking, user, qrBytes);
    }

    private static byte[] GenerateQrCode(Booking booking, string frontendUrl)
    {
        var payload = $"{frontendUrl}/#verify/{booking.Id}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);

        return qrCode.GetGraphic(6);
    }

    private static byte[] GeneratePdf(Booking booking, UserInfo user, byte[] qrBytes)
    {
        var movieTitle = booking.Showtime?.Movie?.Title ?? "—";
        var hallName = booking.Showtime?.Hall?.Name ?? "—";
        var startUtc = booking.Showtime?.StartTime ?? DateTime.MinValue;

        // Konverzija u beogradsko vreme za prikaz
        var startLocal = startUtc == DateTime.MinValue
            ? DateTime.MinValue
            : TimeZoneInfo.ConvertTimeFromUtc(startUtc, BelgradeTz);

        var seatLabel = string.Join(", ", booking.BookingSeats.Select(bs => bs.GetSeatLabel()));

        var pageBg = Color.FromHex("#080606");
        var cardBg = Color.FromHex("#1A1111");
        var darkBg = Color.FromHex("#160909");
        var darkBox = Color.FromHex("#241313");
        var border = Color.FromHex("#3A2323");
        var accentRed = Color.FromHex("#CC8B86");
        var accentGold = Color.FromHex("#D8C7A3");
        var textMain = Color.FromHex("#F7EFE7");
        var textMuted = Color.FromHex("#BFA7A1");
        var textSoft = Color.FromHex("#8E7C78");
        var successBg = Color.FromHex("#20291D");
        var successText = Color.FromHex("#82D982");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9).FontColor(textMain));

                page.Content()
                    .Background(pageBg)
                    .Padding(34)
                    .Column(pageColumn =>
                    {
                        pageColumn.Item()
                            .Border(1)
                            .BorderColor(border)
                            .Background(cardBg)
                            .Row(row =>
                            {
                                // LEVI DEO KARTE
                                row.ConstantItem(260)
                                    .Background(darkBg)
                                    .Padding(22)
                                    .Column(left =>
                                    {
                                        left.Item()
                                            .Text("CinemaVerse")
                                            .FontSize(20)
                                            .Bold()
                                            .FontColor(accentGold);

                                        left.Item()
                                            .PaddingTop(3)
                                            .Text("Cinema Ticket")
                                            .FontSize(9)
                                            .FontColor(accentRed);

                                        left.Item()
                                            .PaddingTop(18)
                                            .LineHorizontal(1)
                                            .LineColor(Color.FromHex("#4A2C2C"));

                                        left.Item()
                                            .PaddingTop(18)
                                            .Column(info =>
                                            {
                                                LeftLabelValue(info, "Movie", movieTitle, textMain, accentRed);
                                                LeftLabelValue(info, "Date", startLocal.ToString("dd.MM.yyyy"), textMain, textSoft);
                                                LeftLabelValue(info, "Time", startLocal.ToString("HH:mm"), textMain, textSoft);
                                                LeftLabelValue(info, "Hall", hallName, textMain, textSoft);
                                                LeftLabelValue(info, "Seats", seatLabel, textMain, textSoft);
                                                LeftLabelValue(info, "Total", $"€{booking.TotalPrice:F2}", accentRed, textSoft);
                                            });

                                        left.Item()
                                            .PaddingTop(22)
                                            .Background(Color.FromHex("#100707"))
                                            .Border(1)
                                            .BorderColor(Color.FromHex("#2C1919"))
                                            .Padding(12)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("BOOKING")
                                                    .FontSize(7)
                                                    .FontColor(textSoft)
                                                    .LetterSpacing(3);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text($"#{booking.Id:D8}")
                                                    .FontSize(18)
                                                    .Bold()
                                                    .FontColor(accentRed);
                                            });
                                    });

                                // DESNI DEO KARTE
                                row.RelativeItem()
                                    .Background(Color.FromHex("#0F0A0A"))
                                    .Padding(24)
                                    .Column(right =>
                                    {
                                        // Header
                                        right.Item()
                                            .Row(header =>
                                            {
                                                header.RelativeItem()
                                                    .Column(hc =>
                                                    {
                                                        hc.Item()
                                                            .Text(movieTitle)
                                                            .FontSize(24)
                                                            .Bold()
                                                            .FontColor(textMain);

                                                        hc.Item()
                                                            .PaddingTop(4)
                                                            .Text(startLocal.ToString(
                                                                "dddd, dd MMMM yyyy",
                                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")))
                                                            .FontSize(10)
                                                            .FontColor(textMuted);
                                                    });

                                                header.AutoItem()
                                                    .Background(successBg)
                                                    .PaddingHorizontal(12)
                                                    .PaddingVertical(7)
                                                    .Text("CONFIRMED")
                                                    .FontSize(9)
                                                    .Bold()
                                                    .FontColor(successText)
                                                    .LetterSpacing(1);
                                            });

                                        right.Item()
                                            .PaddingTop(18)
                                            .LineHorizontal(1)
                                            .LineColor(border);

                                        // Srednji deo: detalji + QR kod
                                        right.Item()
                                            .PaddingTop(20)
                                            .Row(content =>
                                            {
                                                content.RelativeItem()
                                                    .Column(details =>
                                                    {
                                                        details.Item()
                                                            .Text("CUSTOMER")
                                                            .FontSize(7)
                                                            .FontColor(textSoft)
                                                            .LetterSpacing(4);

                                                        details.Item()
                                                            .PaddingTop(8)
                                                            .Background(darkBox)
                                                            .Border(1)
                                                            .BorderColor(border)
                                                            .Padding(12)
                                                            .Column(uc =>
                                                            {
                                                                DetailRow(uc, "Name", user.FullName, textMuted, textMain);
                                                                DetailRow(uc, "Email", user.Email, textMuted, textMain);
                                                            });

                                                        details.Item()
                                                            .PaddingTop(16)
                                                            .Text("SCREENING")
                                                            .FontSize(7)
                                                            .FontColor(textSoft)
                                                            .LetterSpacing(4);

                                                        details.Item()
                                                            .PaddingTop(8)
                                                            .Background(darkBox)
                                                            .Border(1)
                                                            .BorderColor(border)
                                                            .Padding(12)
                                                            .Column(pc =>
                                                            {
                                                                DetailRow(pc, "Movie", movieTitle, textMuted, textMain);
                                                                DetailRow(pc, "Hall", hallName, textMuted, textMain);
                                                                DetailRow(pc, "Seats", seatLabel, textMuted, textMain);
                                                                DetailRow(pc, "Booked", booking.CreatedAt.ToString("dd.MM.yyyy HH:mm") + " UTC", textMuted, textMain);
                                                                DetailRow(pc, "Price", $"€{booking.TotalPrice:F2}", textMuted, accentRed);
                                                            });
                                                    });

                                                content.ConstantItem(190)
                                                    .PaddingLeft(24)
                                                    .Column(qrSection =>
                                                    {
                                                        qrSection.Item()
                                                            .AlignCenter()
                                                            .Background(Colors.White)
                                                            .Padding(10)
                                                            .Width(120)
                                                            .Height(120)
                                                            .Image(qrBytes);

                                                        qrSection.Item()
                                                            .PaddingTop(8)
                                                            .AlignCenter()
                                                            .Text("Scan to check in")
                                                            .FontSize(8)
                                                            .FontColor(textMuted);

                                                        qrSection.Item()
                                                            .PaddingTop(16)
                                                            .AlignCenter()
                                                            .Text($"Booking #{booking.Id:D8}")
                                                            .FontSize(11)
                                                            .Bold()
                                                            .FontColor(accentRed);

                                                        qrSection.Item()
                                                            .PaddingTop(5)
                                                            .AlignCenter()
                                                            .Text("Present this ticket at the entrance.")
                                                            .FontSize(8)
                                                            .FontColor(textMuted);

                                                        qrSection.Item()
                                                            .PaddingTop(14)
                                                            .Border(1)
                                                            .BorderColor(border)
                                                            .Background(Color.FromHex("#140B0B"))
                                                            .Padding(9)
                                                            .AlignCenter()
                                                            .Text($"€{booking.TotalPrice:F2}")
                                                            .FontSize(18)
                                                            .Bold()
                                                            .FontColor(accentRed);
                                                    });
                                            });

                                        // Footer
                                        right.Item()
                                            .PaddingTop(18)
                                            .LineHorizontal(1)
                                            .LineColor(border);

                                        right.Item()
                                            .PaddingTop(10)
                                            .Row(footer =>
                                            {
                                                footer.RelativeItem()
                                                    .Text("© CinemaVerse — automated message")
                                                    .FontSize(7)
                                                    .FontColor(Color.FromHex("#5F4A4A"));

                                                footer.AutoItem()
                                                    .Text("Valid only for selected screening and seats.")
                                                    .FontSize(7)
                                                    .FontColor(Color.FromHex("#5F4A4A"));
                                            });
                                    });
                            });

                        pageColumn.Item()
                            .Height(5)
                            .Background(accentRed);
                    });
            });
        }).GeneratePdf();
    }

    private static void LeftLabelValue(
        ColumnDescriptor col,
        string label,
        string value,
        string valueColor,
        string labelColor)
    {
        col.Item()
            .PaddingTop(9)
            .Column(c =>
            {
                c.Item()
                    .Text(label.ToUpper())
                    .FontSize(6)
                    .FontColor(labelColor)
                    .LetterSpacing(4);

                c.Item()
                    .PaddingTop(3)
                    .Text(value)
                    .FontSize(11)
                    .Bold()
                    .FontColor(valueColor);
            });
    }

    private static void DetailRow(
        ColumnDescriptor col,
        string label,
        string value,
        string labelColor,
        string valueColor)
    {
        col.Item()
            .PaddingTop(5)
            .Row(r =>
            {
                r.ConstantItem(70)
                    .Text(label + ":")
                    .FontSize(8)
                    .FontColor(labelColor);

                r.RelativeItem()
                    .Text(value)
                    .FontSize(8)
                    .Bold()
                    .FontColor(valueColor);
            });
    }
}