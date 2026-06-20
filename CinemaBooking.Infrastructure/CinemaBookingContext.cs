using CinemaBooking.Domain.Models;
using CinemaBooking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure;

public class CinemaBookingContext : IdentityDbContext<ApplicationUser>
{
    public CinemaBookingContext(DbContextOptions<CinemaBookingContext> options) : base(options)
    {
    }

    public CinemaBookingContext()
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Hall> Halls { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingSeat> BookingSeats { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public DbSet<SeatLock> SeatLocks { get; set; }

    // Nema više DbSet<User> — Identity sada upravlja korisnicima

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=CinemaBookingDb;Trusted_Connection=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
            entity.Property(m => m.Description).HasMaxLength(1000);
            entity.Property(m => m.Genre).HasMaxLength(100);
            entity.Property(m => m.Rating).HasColumnType("decimal(3,1)");
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(h => h.Name).IsUnique();
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Row).HasMaxLength(5).IsRequired();
            entity.Property(s => s.SeatType).HasConversion<string>();

            entity.HasOne(s => s.Hall)
                  .WithMany(h => h.Seats)
                  .HasForeignKey(s => s.HallId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Price).HasColumnType("decimal(18,2)").IsRequired();

            //OPTIMISTIC CONCURRENCY
            // Govori EF Core-u da koristi RowVersion kolonu kao concurrency token.
            // SQL Server ce automatski azurirati vrednost pri svakom UPDATE-u.
            entity.Property(s => s.RowVersion)
                  .IsRowVersion()
                  .IsConcurrencyToken();
            
            entity.HasOne(s => s.Movie)
                  .WithMany(m => m.Showtimes)
                  .HasForeignKey(s => s.MovieId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.Hall)
                  .WithMany(h => h.Showtimes)
                  .HasForeignKey(s => s.HallId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(b => b.Status).HasConversion<string>();

            entity.Property(b => b.UserId).HasMaxLength(450).IsRequired();

            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Showtime)
                  .WithMany(s => s.Bookings)
                  .HasForeignKey(b => b.ShowtimeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingSeat>(entity =>
        {
            entity.HasKey(bs => bs.Id);
            entity.Property(bs => bs.Price).HasColumnType("decimal(18,2)").IsRequired();

            entity.HasOne(bs => bs.Booking)
                  .WithMany(b => b.BookingSeats)
                  .HasForeignKey(bs => bs.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bs => bs.Seat)
                  .WithMany(s => s.BookingSeats)
                  .HasForeignKey(bs => bs.SeatId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(bs => new { bs.BookingId, bs.SeatId }).IsUnique();
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(p => p.Status).HasConversion<string>();
            entity.Property(p => p.Method).HasConversion<string>();

            entity.HasOne(p => p.Booking)
                  .WithOne(b => b.Payment)
                  .HasForeignKey<Payment>(p => p.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        //SeatLock konfiguracija
        modelBuilder.Entity<SeatLock>(entity =>
        {
            entity.HasKey(sl => sl.Id);

            entity.Property(sl => sl.UserId).HasMaxLength(450).IsRequired();

            // Jedan korisnik moze imati samo jedan aktivan lock po sedištu i prikazivanju.
            // Unique indeks sprecava duplikate na nivou baze.
            entity.HasIndex(sl => new { sl.SeatId, sl.ShowtimeId, sl.UserId })
                  .IsUnique()
                  .HasDatabaseName("IX_SeatLocks_SeatShowtimeUser");

            entity.HasOne(sl => sl.Seat)
                  .WithMany()
                  .HasForeignKey(sl => sl.SeatId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sl => sl.Showtime)
                  .WithMany()
                  .HasForeignKey(sl => sl.ShowtimeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}