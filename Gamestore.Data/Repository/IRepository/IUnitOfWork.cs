using Gamestore.Entities;

namespace Gamestore.Data.Repository.IRepository;

public interface IUnitOfWork
{
    IGameRepository Games { get; }

    IGameGenreRepository GameGenres { get; }

    IGamePlatformRepository GamePlatforms { get; }

    IRepository<Genre> Genres { get; }

    IRepository<Platform> Platforms { get; }

    IPublisherRepository Publishers { get; }

    Task CompleteAsync();
}
