using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services;
using Gamestore.Services.Dto;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Services.Tests;

public class GameServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var gameServiceMock = new Mock<ILogger<GameService>>();
        _gameService = new GameService(_unitOfWorkMock.Object, null, gameServiceMock.Object);
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnAllGames()
    {
        // Arrange
        var game1 = new Game
        {
            Id = Guid.NewGuid(),
            Key = "game-key-1",
            Name = "Game 1",
            Description = "Description for Game 1",
        };
        var game2 = new Game
        {
            Id = Guid.NewGuid(),
            Key = "game-key-2",
            Name = "Game 2",
            Description = "Description for Game 2",
        };

        var games = new List<Game> { game1, game2 };

        _unitOfWorkMock.Setup(u => u.Games.GetAllAsync()).ReturnsAsync(games);

        // Act
        var gamesList = await _gameService.GetAllGames();

        var gameDtos = new List<GameDto>();
        foreach (var gameDto in gamesList)
        {
            gameDtos.Add(gameDto);
        }

        // Assert
        Assert.Equal(2, gameDtos.Count);
        Assert.Contains(gameDtos, dto => dto.Key == game1.Key);
        Assert.Contains(gameDtos, dto => dto.Key == game2.Key);
    }

    [Fact]
    public async Task GetGameById_ShouldReturnGameDto_WhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Key = "game-key",
            Name = "Test Game",
            Description = "Test Description",
        };

        _unitOfWorkMock.Setup(u => u.Games.GetByIdAsync(gameId)).ReturnsAsync(game);

        // Act
        var result = await _gameService.GetGameById(gameId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(game.Name, result.Name);
        Assert.Equal(game.Key, result.Key);
        Assert.Equal(game.Description, result.Description);
    }

    [Fact]
    public async Task GetGameByKey_ShouldReturnGameDto_WhenGameExists()
    {
        // Arrange
        var gameKey = "game-key";
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Key = gameKey,
            Name = "Test Game",
            Description = "Test Description",
        };

        // Mockowanie repozytorium
        _unitOfWorkMock.Setup(u => u.Games.GetKeyAsync(gameKey)).ReturnsAsync(game);

        // Act
        var result = await _gameService.GetGameByKey(gameKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameKey, result.Key);
        Assert.Equal(game.Name, result.Name);
        Assert.Equal(game.Description, result.Description);
    }

    [Fact]
    public async Task CreateGameFileAsync_ShouldCreateFileAndReturnPath()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            Name = "TestGame",
            Description = "Test Desc",
            Key = "test-key",
        };

        _unitOfWorkMock
            .Setup(u => u.Games.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        var expectedFileName = $"{game.Name}_{game.Id}.txt";
        var expectedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "GameFiles");
        var expectedFilePath = Path.Combine(expectedDirectory, expectedFileName);

        // Cleanup before test
        if (File.Exists(expectedFilePath))
        {
            File.Delete(expectedFilePath);
        }

        if (Directory.Exists(expectedDirectory))
        {
            Directory.Delete(expectedDirectory, recursive: true);
        }

        // Act
        var filePath = await _gameService.CreateGameFileAsync(gameId);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Equal(expectedFilePath, filePath);

        var content = await File.ReadAllTextAsync(filePath);
        Assert.Contains(game.Name, content);

        // Cleanup after test
        File.Delete(filePath);
        Directory.Delete(expectedDirectory);
    }

    [Fact]
    public async Task GetTotalGamesCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var expectedCount = 5;
        _unitOfWorkMock
            .Setup(u => u.Games.CountAsync())
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _gameService.GetTotalGamesCountAsync();

        // Assert
        Assert.Equal(expectedCount, result);
        _unitOfWorkMock.Verify(u => u.Games.CountAsync(), Times.Once);
    }
}