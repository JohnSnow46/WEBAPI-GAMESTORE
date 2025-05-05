using Gamestore.Entities;
using Gamestore.Services.Dto.GamesDto;

namespace Gamestore.Services.IServices;

public interface IGameService
{
    Task<GameDtoUpdate> GetGameByKey(string key);

    Task<GameDtoCreate> GetGameById(Guid id);

    Task<Game> DeleteGameAsync(string key);

    Task<IEnumerable<GameDtoCreate>> GetAllGames();

    Task<string> CreateGameFileAsync(Guid gameId);

    Task<IEnumerable<GameDtoCreate>> GetGamesByPlatformAsync(Guid platformId);

    Task<IEnumerable<GameDtoCreate>> GetGamesByGenreAsync(Guid genreId);

    Task<int> GetTotalGamesCountAsync();

    Task<GameRequestDto> AddGameAsync(GameRequestDto gameRequest);

    Task<GameDtoUpdate> UpdateGameAsync(string key, GameCreateUpdateDto gameRequest);
}
