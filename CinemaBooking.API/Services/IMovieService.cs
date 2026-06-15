using CinemaBooking.API.DTOs.Common;
using CinemaBooking.API.DTOs.Movies;

namespace CinemaBooking.API.Services;

public interface IMovieService
{
    PagedResult<MovieDto> GetAll(MovieSearchRequest request);
    MovieDto? GetById(long id);
    MovieDto Create(CreateMovieRequest request);
    bool Update(long id, UpdateMovieRequest request);
    bool Delete(long id);
}