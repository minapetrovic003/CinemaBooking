namespace CinemaBooking.Domain.Repositories;

public interface IMovieRepository : IRepository<Movie>
{
    IEnumerable<Movie> Search(string? title, string? genre, decimal? minRating);
    Movie? GetByTitle(string title);
}