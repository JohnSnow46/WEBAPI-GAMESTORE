using Gamestore.Data.Repository.Data;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repository;

public class GameRepository(GameCatalogDbContext context) : Repository<Game>(context), IGameRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<Game?> GetKeyAsync(string key)
    {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        return await _context.Games
            .Include(g => g.GameGenres)
                .ThenInclude(gg => gg.Genre)
            .Include(g => g.GamePlatforms)
                .ThenInclude(gp => gp.Platform)
            .Include(g => g.Publisher)
            .FirstOrDefaultAsync(g => g.Key == key);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }

    public async Task<Game> DeleteGameByKey(Game game)
    {
        _context.Games.Remove(game);
        await _context.SaveChangesAsync();

        return game;
    }

    public async Task<IEnumerable<Game>> GetByPlatformAsync(Guid platformId)
    {
        return await _context.GamePlatforms
                             .Where(gp => gp.PlatformId == platformId)
                             .Select(gp => gp.Game)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetByGenreAsync(Guid genreId)
    {
        return await _context.GameGenres
                             .Where(gg => gg.GenreId == genreId)
                             .Select(gg => gg.Game)
                             .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetByIdsAsync(List<Guid> gameIds)
    {
        return await _context.Games
            .Where(g => gameIds.Contains(g.Id))
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.Games.CountAsync();
    }
}
