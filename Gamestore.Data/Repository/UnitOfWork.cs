using Gamestore.Data.Repository.Data;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;

namespace Gamestore.Data.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly GameCatalogDbContext _context;

    public UnitOfWork(GameCatalogDbContext context)
    {
        _context = context;
        Games = new GameRepository(_context);
        GameGenres = new GameGenreRepository(_context);
        GamePlatforms = new GamePlatformRepository(_context);
        Genres = new Repository<Genre>(_context);
        Platforms = new Repository<Platform>(_context);
        Publishers = new PublisherRepository(_context);
    }

    public IGameRepository Games { get; }

    public IGameGenreRepository GameGenres { get; }

    public IGamePlatformRepository GamePlatforms { get; }

    public IRepository<Genre> Genres { get; }

    public IRepository<Platform> Platforms { get; }

    public IPublisherRepository Publishers { get; }

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }
}
