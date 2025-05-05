using Gamestore.Data.Repository.Data;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repository;

public class GameGenreRepository(GameCatalogDbContext context) : Repository<Genre>(context), IGameGenreRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<List<Genre>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Genres
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    public async Task RemoveRangeAsync(IEnumerable<GameGenre> gameGenres)
    {
        _context.GameGenres.RemoveRange(gameGenres);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GameGenre>> GetByGameIdAsync(Guid gameId)
    {
        return await _context.GameGenres
            .Where(g => g.GameId == gameId)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<GameGenre> gameGenres)
    {
        await _context.GameGenres.AddRangeAsync(gameGenres);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<GameGenre>> GetByGenreIdAsync(Guid genreId)
    {
        return await _context.GameGenres
            .Where(gg => gg.GenreId == genreId)
            .ToListAsync();
    }
}
