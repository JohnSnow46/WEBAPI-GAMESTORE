using Gamestore.Data.Repository.Data;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repository;

public class GamePlatformRepository(GameCatalogDbContext context) : Repository<Platform>(context), IGamePlatformRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<List<Platform>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Platforms
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<GamePlatform> gamePlatforms)
    {
        _context.GamePlatforms.RemoveRange(gamePlatforms);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GamePlatform>> GetByGameIdAsync(Guid gameId)
    {
        return await _context.GamePlatforms
            .Where(gp => gp.GameId == gameId)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<GamePlatform> gamePlatforms)
    {
        await _context.GamePlatforms.AddRangeAsync(gamePlatforms);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<GamePlatform>> GetByPlatformIdAsync(Guid platformId)
    {
        return await _context.GamePlatforms
            .Where(gp => gp.PlatformId == platformId)
            .ToListAsync();
    }
}
