using Gamestore.Entities;

namespace Gamestore.Data.Repository.IRepository;

public interface IGamePlatformRepository
{
    Task<List<Platform>> GetByIdsAsync(List<Guid> ids);

    Task<List<GamePlatform>> GetByGameIdAsync(Guid gameId);

    Task RemoveRangeAsync(IEnumerable<GamePlatform> gamePlatforms);

    Task AddRangeAsync(IEnumerable<GamePlatform> gamePlatforms);

    Task<IEnumerable<GamePlatform>> GetByPlatformIdAsync(Guid platformId);
}
