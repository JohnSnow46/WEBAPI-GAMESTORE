using Gamestore.Services.Dto.GenresDto;

namespace Gamestore.Services.IServices;

public interface IGenreService
{
    Task<GenreGetAllDto> GetGenreById(Guid id);

    Task<GenreDto> UpdateGenre(Guid id, GenreUpdateDto genreRequest);

    Task<GenreDtoCreate> CreateGenre(GenreDtoCreate genreRequest);

    Task<IEnumerable<GenreGetAllDto>> GetAllGenres();

    Task<IEnumerable<GenreDto>> GetSubGenresAsync(Guid id);

    Task<GenreDto> DeleteGenreById(Guid id);

    Task<IEnumerable<GenreDto>> GetGenresByGameKeyAsync(string gameKey);
}