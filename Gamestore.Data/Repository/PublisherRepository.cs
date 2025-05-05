using Gamestore.Data.Repository.Data;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Data.Repository;

public class PublisherRepository(GameCatalogDbContext context) : Repository<Publisher>(context), IPublisherRepository
{
    private readonly GameCatalogDbContext _context = context;

    public async Task<Publisher?> GetByCompanyNameAsync(string companyName)
    {
        return await _context.Publishers
            .Include(p => p.Games)
            .FirstOrDefaultAsync(p => p.CompanyName == companyName);
    }

    public async Task<IEnumerable<Game>> GetGamesByPublisherIdAsync(Guid publisherId)
    {
        return await _context.Games
            .Where(g => g.PublisherId == publisherId)
            .ToListAsync();
    }
}