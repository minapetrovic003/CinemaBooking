using CinemaBooking.Domain;
using CinemaBooking.Domain.Repositories;

namespace CinemaBooking.Infrastructure.Repositories;

public class MovieRepository : Repository<Movie>, IMovieRepository
{
    public MovieRepository(CinemaBookingContext context) : base(context) { }

    public IEnumerable<Movie> Search(string? title, string? genre, decimal? minRating)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(m => m.Title.Contains(title));

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(m => m.Genre.Contains(genre));

        if (minRating.HasValue)
            query = query.Where(m => m.Rating >= minRating.Value);

        return query.OrderBy(m => m.Title).ToList();
    }

    public Movie? GetByTitle(string title) =>
        DbSet.FirstOrDefault(m => m.Title == title);
}