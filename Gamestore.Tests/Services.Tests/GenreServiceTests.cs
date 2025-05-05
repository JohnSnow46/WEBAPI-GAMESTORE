using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Services.Tests;

public class GenreServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GenreService _genreService;

    public GenreServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<GenreService>>();
        _genreService = new GenreService(_unitOfWorkMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnGenreDto_WhenGenreExists()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var genre = new Genre
        {
            Id = genreId,
            Name = "Action",
            ParentGenreId = Guid.NewGuid(),
        };

        _unitOfWorkMock.Setup(u => u.Genres.GetByIdAsync(genreId))
            .ReturnsAsync(genre);

        // Act
        var result = await _genreService.GetGenreById(genreId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(genreId, result.Id);
        Assert.Equal("Action", result.Name);
        Assert.Equal(genre.ParentGenreId, result.ParentGenreId);
    }

    [Fact]
    public async Task GetAllGenres_ShouldReturnGenreDtos_WhenGenresExist()
    {
        // Arrange
        var genres = new List<Genre>
        {
            new() { Id = Guid.NewGuid(), Name = "Action", ParentGenreId = null },
            new() { Id = Guid.NewGuid(), Name = "Adventure", ParentGenreId = null },
        };

        _unitOfWorkMock.Setup(u => u.Genres.GetAllAsync()).ReturnsAsync(genres);

        // Act
        var result = await _genreService.GetAllGenres();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetGenresByGameKeyAsync_ShouldReturnGenreDtos_WhenGameExists()
    {
        // Arrange
        var gameKey = "game123";
        var game = new Game { Id = Guid.NewGuid() };

        var gameGenres = new List<GameGenre>
    {
        new() { GenreId = Guid.NewGuid() },
        new() { GenreId = Guid.NewGuid() },
    };

        var genres = new List<Genre>
    {
        new() { Id = gameGenres[0].GenreId, Name = "Action" },
        new() { Id = gameGenres[1].GenreId, Name = "Adventure" },
    };

        _unitOfWorkMock.Setup(u => u.Games.GetKeyAsync(gameKey)).ReturnsAsync(game);
        _unitOfWorkMock.Setup(u => u.GameGenres.GetByGameIdAsync(game.Id)).ReturnsAsync(gameGenres);
        _unitOfWorkMock.Setup(u => u.GameGenres.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(genres);

        // Act
        var result = await _genreService.GetGenresByGameKeyAsync(gameKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}