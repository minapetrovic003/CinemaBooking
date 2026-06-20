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
        // QR vodi na frontend verify stranicu — korisnik skenira i check-in radi sam
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
        var startTime = booking.Showtime?.StartTime ?? DateTime.MinValue;
        var seatLabel = string.Join(", ", booking.BookingSeats.Select(bs => bs.GetSeatLabel()));

        var darkBg = Color.FromHex("#1a0e0e");
        var accentRed = Color.FromHex("#CC8B86");
        var lightGray = Color.FromHex("#f8f9fa");
        var midGray = Color.FromHex("#6c757d");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Content().Row(row =>
                {
                    // ── Lijeva tamna kolona ──────────────────────────────
                    row.ConstantItem(220)
                        .Background(darkBg)
                        .Padding(24)
                        .Column(left =>
                        {
                            left.Item()
                                .Text("🎬 CinemaVerse")
                                .FontSize(14).Bold().FontColor(Colors.White);

                            left.Item()
                                .PaddingTop(4)
                                .Text("Cinema Ticket")
                                .FontSize(9).FontColor(accentRed);

                            left.Item()
                                .PaddingTop(20)
                                .LineHorizontal(1)
                                .LineColor(Color.FromHex("#33FFFFFF"));

                            left.Item()
                                .PaddingTop(16)
                                .Column(info =>
                                {
                                    LeftLabelValue(info, "Movie", movieTitle, Colors.White, accentRed);
                                    LeftLabelValue(info, "Date", startTime.ToString("dd.MM.yyyy"), Colors.White, Colors.White);
                                    LeftLabelValue(info, "Time", startTime.ToString("HH:mm"), Colors.White, Colors.White);
                                    LeftLabelValue(info, "Hall", hallName, Colors.White, Colors.White);
                                    LeftLabelValue(info, "Seats", seatLabel, Colors.White, Colors.White);
                                });

                            left.Item().Extend();

                            left.Item()
                                .Background(Color.FromHex("#14090a"))
                                .Padding(10)
                                .Column(c =>
                                {
                                    c.Item().Text("BOOKING")
                                        .FontSize(7).FontColor(midGray).LetterSpacing(2);
                                    c.Item().Text($"#{booking.Id:D8}")
                                        .FontSize(16).Bold().FontColor(accentRed);
                                });
                        });

                    // ── Desna bijela kolona ──────────────────────────────
                    row.RelativeItem()
                        .Background(Colors.White)
                        .Column(right =>
                        {
                            right.Item()
                                .Background(lightGray)
                                .Padding(14)
                                .Row(header =>
                                {
                                    header.RelativeItem().Column(hc =>
                                    {
                                        hc.Item().Text(movieTitle)
                                            .FontSize(15).Bold().FontColor(darkBg);
                                        hc.Item().Text(
                                            startTime.ToString("dddd, dd MMMM yyyy",
                                                System.Globalization.CultureInfo.GetCultureInfo("sr-Latn-RS")))
                                            .FontSize(9).FontColor(midGray);
                                    });
                                });

                            right.Item()
                                .Padding(14)
                                .Column(details =>
                                {
                                    details.Item().Text("CUSTOMER")
                                        .FontSize(7).FontColor(midGray).LetterSpacing(1.5f);
                                    details.Item().PaddingTop(6).Column(uc =>
                                    {
                                        DetailRow(uc, "Name", user.FullName);
                                        DetailRow(uc, "Email", user.Email);
                                    });

                                    details.Item().PaddingTop(10).Text("SCREENING DETAILS")
                                        .FontSize(7).FontColor(midGray).LetterSpacing(1.5f);
                                    details.Item().PaddingTop(6).Column(pc =>
                                    {
                                        DetailRow(pc, "Movie", movieTitle);
                                        DetailRow(pc, "Hall", hallName);
                                        DetailRow(pc, "Seats", seatLabel);
                                        DetailRow(pc, "Booked on", booking.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                                        DetailRow(pc, "Total price", $"€{booking.TotalPrice:F2}");
                                    });
                                });

                            right.Item().Extend();

                            right.Item()
                                .Padding(14)
                                .Row(footer =>
                                {
                                    footer.AutoItem().Column(qrCol =>
                                    {
                                        qrCol.Item().Width(75).Height(75).Image(qrBytes);
                                        qrCol.Item().PaddingTop(3)
                                            .Text("Scan to check in")
                                            .FontSize(6).FontColor(midGray).AlignCenter();
                                    });

                                    footer.RelativeItem().PaddingLeft(12).Column(fc =>
                                    {
                                        fc.Item().Extend();
                                        fc.Item().Text($"Booking #{booking.Id:D8}")
                                            .FontSize(8).Bold().FontColor(darkBg);
                                        fc.Item().Text("Please present this ticket at the entrance.")
                                            .FontSize(7).FontColor(midGray);
                                        fc.Item().PaddingTop(4)
                                            .Text("© CinemaVerse — automated message")
                                            .FontSize(6).FontColor(Color.FromHex("#dee2e6"));
                                    });
                                });

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
        col.Item().PaddingTop(8).Column(c =>
        {
            c.Item().Text(label.ToUpper())
                .FontSize(7).FontColor(labelColor).LetterSpacing(1);
            c.Item().PaddingTop(2).Text(value)
                .FontSize(10).Bold().FontColor(valueColor);
        });
    }

    private static void DetailRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingTop(4).Row(r =>
        {
            r.ConstantItem(90).Text(label + ":")
                .FontSize(8).FontColor(Color.FromHex("#6c757d"));
            r.RelativeItem().Text(value)
                .FontSize(8).Bold().FontColor(Color.FromHex("#1a0e0e"));
        });
    }
}