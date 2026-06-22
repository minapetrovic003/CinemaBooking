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

        var darkBg = Color.FromHex("#1a0e0e");
        var accentRed = Color.FromHex("#CC8B86");
        var midGray = Color.FromHex("#6c757d");
        var lightBg = Color.FromHex("#f8f9fa");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // Jedan A5-landscape list
                page.Size(PageSizes.A5.Landscape());
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

                page.Content().Row(row =>
                {
                    // ── Lijeva tamna kolona ──────────────────────────────
                    row.ConstantItem(200)
                        .Background(darkBg)
                        .Padding(18)
                        .Column(left =>
                        {
                            left.Item()
                                .Text("CinemaVerse")
                                .FontSize(13).Bold().FontColor(Colors.White);

                            left.Item()
                                .PaddingTop(2)
                                .Text("Cinema Ticket")
                                .FontSize(8).FontColor(accentRed);

                            left.Item()
                                .PaddingTop(14)
                                .LineHorizontal(1)
                                .LineColor(Color.FromHex("#33FFFFFF"));

                            left.Item()
                                .PaddingTop(10)
                                .Column(info =>
                                {
                                    LeftLabelValue(info, "Movie", movieTitle, Colors.White, accentRed);
                                    LeftLabelValue(info, "Date",
                                        startLocal.ToString("dd.MM.yyyy"),
                                        Colors.White, Colors.White);
                                    LeftLabelValue(info, "Time",
                                        startLocal.ToString("HH:mm"),
                                        Colors.White, Colors.White);
                                    LeftLabelValue(info, "Hall", hallName, Colors.White, Colors.White);
                                    LeftLabelValue(info, "Seats", seatLabel, Colors.White, Colors.White);
                                    LeftLabelValue(info, "Total",
                                        $"€{booking.TotalPrice:F2}",
                                        Colors.White, Colors.White);
                                });

                            left.Item().Extend();

                            left.Item()
                                .Background(Color.FromHex("#14090a"))
                                .Padding(8)
                                .Column(c =>
                                {
                                    c.Item().Text("BOOKING")
                                        .FontSize(7).FontColor(midGray).LetterSpacing(2);
                                    c.Item().Text($"#{booking.Id:D8}")
                                        .FontSize(14).Bold().FontColor(accentRed);
                                });
                        });

                    // ── Desna bijela kolona ──────────────────────────────
                    row.RelativeItem()
                        .Background(Colors.White)
                        .Column(right =>
                        {
                            // Header
                            right.Item()
                                .Background(lightBg)
                                .Padding(12)
                                .Column(hc =>
                                {
                                    hc.Item().Text(movieTitle)
                                        .FontSize(14).Bold().FontColor(darkBg);
                                    hc.Item().Text(
                                        startLocal.ToString("dddd, dd MMMM yyyy",
                                            System.Globalization.CultureInfo.GetCultureInfo("sr-Latn-RS")))
                                        .FontSize(8).FontColor(midGray);
                                });

                            // Customer + details (kompaktno, sve na jednoj stranici)
                            right.Item()
                                .Padding(12)
                                .Column(details =>
                                {
                                    details.Item().Text("CUSTOMER")
                                        .FontSize(6).FontColor(midGray).LetterSpacing(1.5f);
                                    details.Item().PaddingTop(4).Column(uc =>
                                    {
                                        DetailRow(uc, "Name", user.FullName);
                                        DetailRow(uc, "Email", user.Email);
                                    });

                                    details.Item().PaddingTop(8).Text("SCREENING")
                                        .FontSize(6).FontColor(midGray).LetterSpacing(1.5f);
                                    details.Item().PaddingTop(4).Column(pc =>
                                    {
                                        DetailRow(pc, "Movie", movieTitle);
                                        DetailRow(pc, "Hall", hallName);
                                        DetailRow(pc, "Seats", seatLabel);
                                        DetailRow(pc, "Booked", booking.CreatedAt
                                            .ToString("dd.MM.yyyy HH:mm") + " UTC");
                                        DetailRow(pc, "Price", $"€{booking.TotalPrice:F2}");
                                    });
                                });

                            right.Item().Extend();

                            // Footer: QR kod + info
                            right.Item()
                                .Padding(12)
                                .Row(footer =>
                                {
                                    footer.AutoItem().Column(qrCol =>
                                    {
                                        qrCol.Item().Width(70).Height(70).Image(qrBytes);
                                        qrCol.Item().PaddingTop(2)
                                            .Text("Scan to check in")
                                            .FontSize(5).FontColor(midGray).AlignCenter();
                                    });

                                    footer.RelativeItem().PaddingLeft(10).Column(fc =>
                                    {
                                        fc.Item().Extend();
                                        fc.Item().Text($"Booking #{booking.Id:D8}")
                                            .FontSize(7).Bold().FontColor(darkBg);
                                        fc.Item().Text("Present this ticket at the entrance.")
                                            .FontSize(6).FontColor(midGray);
                                        fc.Item().PaddingTop(3)
                                            .Text("© CinemaVerse — automated message")
                                            .FontSize(5).FontColor(Color.FromHex("#dee2e6"));
                                    });
                                });

                            // Dekorativna linija na dnu
                            right.Item().Height(5).Background(accentRed);
                        });
                });
            });
        }).GeneratePdf();
    }

    private static void LeftLabelValue(
        ColumnDescriptor col, string label, string value,
        string valueColor, string labelColor)
    {
        col.Item().PaddingTop(6).Column(c =>
        {
            c.Item().Text(label.ToUpper())
                .FontSize(6).FontColor(labelColor).LetterSpacing(1);
            c.Item().PaddingTop(1).Text(value)
                .FontSize(9).Bold().FontColor(valueColor);
        });
    }

    private static void DetailRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingTop(3).Row(r =>
        {
            r.ConstantItem(55).Text(label + ":")
                .FontSize(7).FontColor(Color.FromHex("#6c757d"));
            r.RelativeItem().Text(value)
                .FontSize(7).Bold().FontColor(Color.FromHex("#1a0e0e"));
        });
    }
}