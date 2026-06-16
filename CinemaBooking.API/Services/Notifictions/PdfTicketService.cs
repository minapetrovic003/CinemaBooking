using CinemaBooking.Domain;
using CinemaBooking.Infrastructure.Identity;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CinemaBooking.API.Services.Notifications;

public class PdfTicketService : IPdfTicketService
{
    public PdfTicketService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateTicket(Booking booking, ApplicationUser user)
    {
        var qrBytes = GenerateQrCode(booking);
        return GeneratePdf(booking, user, qrBytes);
    }

    private static byte[] GenerateQrCode(Booking booking)
    {
        var seats = string.Join(",",
            booking.BookingSeats.Select(bs => bs.GetSeatLabel()));

        var payload =
            $"BOOKING:{booking.Id}|" +
            $"MOVIE:{booking.Showtime?.Movie?.Title}|" +
            $"TIME:{booking.Showtime?.StartTime:yyyy-MM-ddTHH:mm}|" +
            $"HALL:{booking.Showtime?.Hall?.Name}|" +
            $"SEATS:{seats}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrData);

        return qrCode.GetGraphic(6);
    }

    private static byte[] GeneratePdf(
        Booking booking,
        ApplicationUser user,
        byte[] qrBytes)
    {
        var movieTitle = booking.Showtime?.Movie?.Title ?? "—";
        var hallName = booking.Showtime?.Hall?.Name ?? "—";
        var startTime = booking.Showtime?.StartTime ?? DateTime.MinValue;

        var seatLabel = string.Join(", ",
            booking.BookingSeats.Select(bs => bs.GetSeatLabel()));

        var darkBlue = Color.FromHex("#1a1a2e");
        var accentRed = Color.FromHex("#e94560");
        var lightGray = Color.FromHex("#f8f9fa");
        var midGray = Color.FromHex("#6c757d");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(0);

                page.DefaultTextStyle(x =>
                    x.FontFamily("Arial")
                     .FontSize(10));

                page.Content().Row(row =>
                {
                    row.ConstantItem(220)
                        .Background(darkBlue)
                        .Padding(24)
                        .Column(left =>
                        {
                            left.Item()
                                .Text("🎬 CinemaBooking")
                                .FontSize(14)
                                .Bold()
                                .FontColor(Colors.White);

                            left.Item()
                                .PaddingTop(4)
                                .Text("Vaša bioskopska karta")
                                .FontSize(9)
                                .FontColor(accentRed);

                            left.Item()
                                .PaddingTop(24)
                                .LineHorizontal(1)
                                .LineColor(Color.FromHex("#33FFFFFF"));

                            left.Item()
                                .PaddingTop(20)
                                .Column(info =>
                                {
                                    LeftLabelValue(info, "Film", movieTitle, Colors.White, accentRed);
                                    LeftLabelValue(info, "Datum", startTime.ToString("dd.MM.yyyy"), Colors.White, Colors.White);
                                    LeftLabelValue(info, "Vrijeme", startTime.ToString("HH:mm"), Colors.White, Colors.White);
                                    LeftLabelValue(info, "Sala", hallName, Colors.White, Colors.White);
                                    LeftLabelValue(info, "Sjedišta", seatLabel, Colors.White, Colors.White);
                                });

                            left.Item().Extend();

                            left.Item()
                                .Background(Color.FromHex("#16213e"))
                                .Padding(10)
                                .Column(c =>
                                {
                                    c.Item()
                                        .Text("REZERVACIJA")
                                        .FontSize(7)
                                        .FontColor(midGray)
                                        .LetterSpacing(2);

                                    c.Item()
                                        .Text($"#{booking.Id:D8}")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(accentRed);
                                });
                        });

                    row.RelativeItem()
                        .Background(Colors.White)
                        .Column(right =>
                        {
                            right.Item()
                                .Background(lightGray)
                                .Padding(16)
                                .Row(header =>
                                {
                                    header.RelativeItem()
                                        .Column(hc =>
                                        {
                                            hc.Item()
                                                .Text(movieTitle)
                                                .FontSize(16)
                                                .Bold()
                                                .FontColor(darkBlue);

                                            hc.Item()
                                                .Text(
                                                    startTime.ToString(
                                                        "dddd, dd MMMM yyyy",
                                                        System.Globalization.CultureInfo.GetCultureInfo("sr-Latn-RS")))
                                                .FontSize(9)
                                                .FontColor(midGray);
                                        });
                                });

                            right.Item()
                                .Padding(16)
                                .Column(details =>
                                {
                                    details.Item()
                                        .Text("PODACI O KORISNIKU")
                                        .FontSize(7)
                                        .FontColor(midGray)
                                        .LetterSpacing(1.5f);

                                    details.Item()
                                        .PaddingTop(8)
                                        .Column(uc =>
                                        {
                                            DetailRow(uc, "Ime i prezime", user.GetFullName());
                                            DetailRow(uc, "Email", user.Email ?? "—");
                                        });

                                    details.Item()
                                        .PaddingTop(14)
                                        .Text("DETALJI PROJEKCIJE")
                                        .FontSize(7)
                                        .FontColor(midGray)
                                        .LetterSpacing(1.5f);

                                    details.Item()
                                        .PaddingTop(8)
                                        .Column(pc =>
                                        {
                                            DetailRow(pc, "Film", movieTitle);
                                            DetailRow(pc, "Sala", hallName);
                                            DetailRow(pc, "Sjedišta", seatLabel);
                                            DetailRow(pc, "Datum kreiranja", booking.CreatedAt.ToString("dd.MM.yyyy HH:mm"));
                                            DetailRow(pc, "Ukupna cijena", $"{booking.TotalPrice:C}");
                                        });
                                });

                            right.Item().Extend();

                            right.Item()
                                .Padding(16)
                                .Row(footer =>
                                {
                                    footer.AutoItem()
                                        .Column(qrCol =>
                                        {
                                            qrCol.Item()
                                                .Width(80)
                                                .Height(80)
                                                .Image(qrBytes);

                                            qrCol.Item()
                                                .PaddingTop(4)
                                                .Text("Skeniraj za provjeru")
                                                .FontSize(6)
                                                .FontColor(midGray)
                                                .AlignCenter();
                                        });

                                    footer.RelativeItem()
                                        .PaddingLeft(14)
                                        .Column(fc =>
                                        {
                                            fc.Item().Extend();

                                            fc.Item()
                                                .Text($"Rezervacija #{booking.Id:D8}")
                                                .FontSize(8)
                                                .Bold()
                                                .FontColor(darkBlue);

                                            fc.Item()
                                                .Text("Molimo Vas da ovu kartu pokažete")
                                                .FontSize(7)
                                                .FontColor(midGray);

                                            fc.Item()
                                                .Text("osoblju na ulazu u bioskop.")
                                                .FontSize(7)
                                                .FontColor(midGray);

                                            fc.Item()
                                                .PaddingTop(6)
                                                .Text("© CinemaBooking — automatska poruka")
                                                .FontSize(6)
                                                .FontColor(Color.FromHex("#dee2e6"));
                                        });
                                });

                            right.Item()
                                .Height(6)
                                .Background(accentRed);
                        });
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
            .PaddingTop(10)
            .Column(c =>
            {
                c.Item()
                    .Text(label.ToUpper())
                    .FontSize(7)
                    .FontColor(labelColor)
                    .LetterSpacing(1);

                c.Item()
                    .PaddingTop(2)
                    .Text(value)
                    .FontSize(11)
                    .Bold()
                    .FontColor(valueColor);
            });
    }

    private static void DetailRow(
        ColumnDescriptor col,
        string label,
        string value)
    {
        col.Item()
            .PaddingTop(5)
            .Row(r =>
            {
                r.ConstantItem(95)
                    .Text(label + ":")
                    .FontSize(8)
                    .FontColor(Color.FromHex("#6c757d"));

                r.RelativeItem()
                    .Text(value)
                    .FontSize(8)
                    .Bold()
                    .FontColor(Color.FromHex("#1a1a2e"));
            });
    }
}