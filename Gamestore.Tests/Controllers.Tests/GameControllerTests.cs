using Gamestore.Entities;
using Gamestore.Services.Dto;
using Gamestore.Services.IServices;
using Gamestore.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace Gamestore.Tests.Controllers.Tests;
public class GameControllerTests
{
    private readonly Mock<IGameService> _gameServiceMock;
    private readonly GameController _controller;

    public GameControllerTests()
    {
        _gameServiceMock = new Mock<IGameService>();
        var loggerMock = new Mock<ILogger<GameController>>();
        _controller = new GameController(_gameServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateGame_ShouldReturnOk_WhenGameIsCreatedOrUpdated()
    {
        // Arrange
        var game = new Game { Id = Guid.NewGuid(), Name = "Action Game", Key = "action-game" };
        var gameDto = new GameDto { Name = game.Name, Key = game.Key };
        var request = new GameRequestDto { Game = gameDto };

        _gameServiceMock
            .Setup(s => s.AddOrUpdateGameAsync(request))
            .ReturnsAsync(game);

        // Act
        var result = await _controller.CreateOrUpdateGame(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGame = Assert.IsType<Game>(okResult.Value);
        Assert.Equal(game.Id, returnedGame.Id);
        Assert.Equal(game.Name, returnedGame.Name);
    }

    [Fact]
    public async Task CreateOrUpdateGame_ShouldReturnNotFound_WhenGameNotFound()
    {
        // Arrange
        var gameDto = new GameDto { Name = "NonExistentGame", Key = "nonexistent-key" };
        var request = new GameRequestDto { Game = gameDto };

        _gameServiceMock
            .Setup(s => s.AddOrUpdateGameAsync(request))
            .Returns(Task.FromResult<Game>(null));

        // Act
        var result = await _controller.CreateOrUpdateGame(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateOrUpdateGame_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var gameDto = new GameDto { Name = "Broken Game", Key = "broken-game" };
        var request = new GameRequestDto { Game = gameDto };

        _gameServiceMock
            .Setup(s => s.AddOrUpdateGameAsync(request))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.CreateOrUpdateGame(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Unexpected error", errorDict["Details"]);
    }

    [Fact]
    public async Task GetGameByKey_ShouldReturnOk_WhenGameExists()
    {
        // Arrange
        var key = "test-key";
        var expectedGame = new GameDto
        {
            Name = "Test Game",
            Key = key,
            Description = "Test Description",
        };
        _gameServiceMock
            .Setup(s => s.GetGameByKey(key))
            .ReturnsAsync(expectedGame);

        // Act
        var result = await _controller.GetGameByKey(key);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGame = Assert.IsType<GameDto>(okResult.Value);
        Assert.Equal(expectedGame.Name, returnedGame.Name);
        Assert.Equal(expectedGame.Description, returnedGame.Description);
        Assert.Equal(expectedGame.Key, returnedGame.Key);
    }

    [Fact]
    public async Task GetGameByKey_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var key = string.Empty;
        _gameServiceMock
            .Setup(s => s.GetGameByKey(It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Key cannot be empty"));

        // Act
        var result = await _controller.GetGameByKey(key);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Key cannot be empty", errorDict["Details"]);
    }

    [Fact]
    public async Task GetGameByKey_ShouldReturnNotFound_WhenGameDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";
        _gameServiceMock
            .Setup(s => s.GetGameByKey(key))
            .Returns(Task.FromResult<GameDto>(null));

        // Act
        var result = await _controller.GetGameByKey(key);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetGameById_ShouldReturnOk_WhenGameExists()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var expectedGame = new GameDto
        {
            Name = "Test Game",
            Key = "test-key",
            Description = "Test Description",
        };
        _gameServiceMock
           .Setup(s => s.GetGameById(gameId))
            .ReturnsAsync(expectedGame);

        // Act
        var result = await _controller.GetGameById(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGame = Assert.IsType<GameDto>(okResult.Value);
        Assert.Equal(expectedGame.Name, returnedGame.Name);
        Assert.Equal(expectedGame.Description, returnedGame.Description);
    }

    [Fact]
    public async Task GetGameById_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _gameServiceMock
           .Setup(s => s.GetGameById(It.IsAny<Guid>()))
           .ThrowsAsync(new ArgumentException("Id cannot be empty"));

        // Act
        var result = await _controller.GetGameById(gameId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Id cannot be empty", errorDict["Details"]);
    }

    [Fact]
    public async Task GetGameById_ShouldReturnNotFound_WhenGameDoesNotExist()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _gameServiceMock
           .Setup(s => s.GetGameById(gameId))
           .Returns(Task.FromResult<GameDto>(null));

        // Act
        var result = await _controller.GetGameById(gameId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteGameByKey_ShouldReturnOk_WhenGameIsDeleted()
    {
        // Arrange
        var gameKey = "test-key";
        var expectedGame = new GameDto
        {
            Name = "Test Game",
            Key = gameKey,
            Description = "Test Description",
        };
        _gameServiceMock
            .Setup(s => s.DeleteGameAsync(gameKey))
            .ReturnsAsync(expectedGame);

        // Act
        var result = await _controller.DeleteGameByKey(gameKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGame = Assert.IsType<GameDto>(okResult.Value);
        Assert.Equal(expectedGame.Name, returnedGame.Name);
        Assert.Equal(expectedGame.Description, returnedGame.Description);
    }

    [Fact]
    public async Task DeleteGameByKey_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var gameKey = "test-key";
        _gameServiceMock
            .Setup(s => s.DeleteGameAsync(It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Key cannot be empty"));

        // Act
        var result = await _controller.DeleteGameByKey(gameKey);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Key cannot be empty", errorDict["Details"]);
    }

    [Fact]
    public async Task DeleteGameByKey_ShouldReturnNotFound_WhenGameDoesNotExist()
    {
        // Arrange
        var gameKey = "nonexistent-key";
        _gameServiceMock
            .Setup(s => s.DeleteGameAsync(gameKey))
            .Returns(Task.FromResult<GameDto>(null));

        // Act
        var result = await _controller.DeleteGameByKey(gameKey);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetGamesByPlatformId_ShouldReturnOk_WhenGamesExist()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var games = new List<GameDto>
    {
        new() { Name = "Game 1", Key = "key1" },
        new() { Name = "Game 2", Key = "key2" },
    };
        _gameServiceMock
            .Setup(s => s.GetGamesByPlatformAsync(platformId))
            .ReturnsAsync(games);

        // Act
        var result = await _controller.GetGamesByPlatformId(platformId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<GameDto>>(okResult.Value);
        Assert.Equal(games.Count, returnValue.Count);
    }

    [Fact]
    public async Task GetGamesByPlatformId_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        _gameServiceMock
            .Setup(s => s.GetGamesByPlatformAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ArgumentException("Id cannot be empty"));

        // Act
        var result = await _controller.GetGamesByPlatformId(platformId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Id cannot be empty", errorDict["Details"]);
    }

    [Fact]
    public async Task GetGamesByPlatformId_ShouldReturnNotFound_WhenNoGamesExist()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        _gameServiceMock
            .Setup(s => s.GetGamesByPlatformAsync(platformId))
            .ReturnsAsync((List<GameDto>)null);

        // Act
        var result = await _controller.GetGamesByPlatformId(platformId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetGamesByGenreId_ShouldReturnOk_WhenGamesExist()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        var games = new List<GameDto>
    {
        new() { Name = "Game A", Key = "a-key" },
        new() { Name = "Game B", Key = "b-key" },
    };
        _gameServiceMock
            .Setup(s => s.GetGamesByGenreAsync(genreId))
            .ReturnsAsync(games);

        // Act
        var result = await _controller.GetGamesByGenreId(genreId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<GameDto>>(okResult.Value);
        Assert.Equal(games.Count, returnValue.Count);
    }

    [Fact]
    public async Task GetGamesByGenreId_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        _gameServiceMock
            .Setup(s => s.GetGamesByGenreAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ArgumentException("Id cannot be empty"));

        // Act
        var result = await _controller.GetGamesByGenreId(genreId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Id cannot be empty", errorDict["Details"]);
    }

    [Fact]
    public async Task GetGamesByGenreId_ShouldReturnNotFound_WhenNoGamesExist()
    {
        // Arrange
        var genreId = Guid.NewGuid();
        _gameServiceMock
            .Setup(s => s.GetGamesByGenreAsync(genreId))
            .ReturnsAsync((List<GameDto>)null);

        // Act
        var result = await _controller.GetGamesByGenreId(genreId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnOk_WhenGamesExist()
    {
        // Arrange
        var games = new List<GameDto>
    {
        new() { Name = "Game A", Key = "a-key" },
        new() { Name = "Game B", Key = "b-key" },
    };
        _gameServiceMock
            .Setup(s => s.GetAllGames())
            .ReturnsAsync(games);

        // Act
        var result = await _controller.GetAllGames();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGames = Assert.IsAssignableFrom<IEnumerable<GameDto>>(okResult.Value);
        Assert.Equal(2, returnedGames.Count());
        Assert.Contains(returnedGames, g => g.Name == "Game A");
        Assert.Contains(returnedGames, g => g.Name == "Game B");
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        _gameServiceMock
            .Setup(s => s.GetAllGames())
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _controller.GetAllGames();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Database connection error", errorDict["Details"]);
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnNotFound_WhenNoGamesExist()
    {
        // Arrange
        _gameServiceMock
            .Setup(s => s.GetAllGames())
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetAllGames();

        // Assert
        var statusCodeResult = Assert.IsAssignableFrom<IStatusCodeActionResult>(result);
        Assert.Equal(404, statusCodeResult.StatusCode);
    }
}