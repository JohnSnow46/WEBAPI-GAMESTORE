using Gamestore.Entities;
using Gamestore.Services.Dto;
using Gamestore.Services.IServices;
using Gamestore.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace Gamestore.Tests.Controllers.Tests;

public class PlatformControllerTests
{
    private readonly Mock<IPlatformService> _platformServiceMock;
    private readonly PlatformController _controller;

    public PlatformControllerTests()
    {
        _platformServiceMock = new Mock<IPlatformService>();
        var loggerMock = new Mock<ILogger<PlatformController>>();
        _controller = new PlatformController(_platformServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdatePlatform_ShouldReturnOk_WhenPlatformIsCreatedOrUpdated()
    {
        // Arrange
        var platformDto = new PlatformDto { Id = Guid.NewGuid(), Type = "PlayStation 5" };
        var request = new PlatfromRequestDto { Platform = platformDto };

        _platformServiceMock
            .Setup(s => s.CreateOrUpdatePlatform(request))
            .ReturnsAsync(platformDto);

        // Act
        var result = await _controller.CreateOrUpdatePlatform(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatform = Assert.IsType<PlatformDto>(okResult.Value);
        Assert.Equal(platformDto.Id, returnedPlatform.Id);
        Assert.Equal(platformDto.Type, returnedPlatform.Type);
    }

    [Fact]
    public async Task CreateOrUpdatePlatform_ShouldReturnNotFound_WhenPlatformNotFound()
    {
        // Arrange
        var platformDto = new PlatformDto { Id = Guid.NewGuid(), Type = "NonExistentPlatform" };
        var request = new PlatfromRequestDto { Platform = platformDto };

        _platformServiceMock
            .Setup(s => s.CreateOrUpdatePlatform(request))
            .Returns(Task.FromResult<PlatformDto>(null));

        // Act
        var result = await _controller.CreateOrUpdatePlatform(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateOrUpdatePlatform_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var platformDto = new PlatformDto { Id = Guid.NewGuid(), Type = "Error Platform" };
        var request = new PlatfromRequestDto { Platform = platformDto };

        _platformServiceMock
            .Setup(s => s.CreateOrUpdatePlatform(request))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.CreateOrUpdatePlatform(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Test exception", errorDict["Details"]);
    }

    [Fact]
    public async Task DeletePlatformById_ShouldReturnOk_WhenPlatformIsDeleted()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platform = new Platform { Id = platformId, Type = "Xbox Series X" };

        _platformServiceMock
            .Setup(s => s.DeletePlatformById(platformId))
            .ReturnsAsync(platform);

        // Act
        var result = await _controller.DeletePlatformById(platformId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatform = Assert.IsType<Platform>(okResult.Value);
        Assert.Equal(platform.Id, returnedPlatform.Id);
        Assert.Equal(platform.Type, returnedPlatform.Type);
    }

    [Fact]
    public async Task DeletePlatformById_ShouldReturnNotFound_WhenPlatformDoesNotExist()
    {
        // Arrange
        var platformId = Guid.NewGuid();

        _platformServiceMock
            .Setup(s => s.DeletePlatformById(platformId))
            .Returns(Task.FromResult<Platform>(null));

        // Act
        var result = await _controller.DeletePlatformById(platformId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeletePlatformById_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var platformId = Guid.NewGuid();

        _platformServiceMock
            .Setup(s => s.DeletePlatformById(platformId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.DeletePlatformById(platformId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Test exception", errorDict["Details"]);
    }

    [Fact]
    public async Task GetAllPlatforms_ShouldReturnOk_WhenPlatformsExist()
    {
        // Arrange
        var platforms = new List<PlatformDto>
    {
        new() { Id = Guid.NewGuid(), Type = "PlayStation 5" },
        new() { Id = Guid.NewGuid(), Type = "Xbox Series X" },
    };

        _platformServiceMock
            .Setup(s => s.GetAllPlatformsAsync())
            .ReturnsAsync(platforms);

        // Act
        var result = await _controller.GetAllPlatforms();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPlatforms = Assert.IsAssignableFrom<IEnumerable<PlatformDto>>(okResult.Value);
        Assert.Equal(2, returnedPlatforms.Count());
        Assert.Contains(returnedPlatforms, p => p.Type == "PlayStation 5");
        Assert.Contains(returnedPlatforms, p => p.Type == "Xbox Series X");
    }

    [Fact]
    public async Task GetAllPlatforms_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        _platformServiceMock
            .Setup(s => s.GetAllPlatformsAsync())
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _controller.GetAllPlatforms();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Database connection error", errorDict["Details"]);
    }

    [Fact]
    public async Task GetPlatformById_ShouldReturnOk_WhenPlatformExists()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platformDto = new PlatformDto
        {
            Id = platformId,
            Type = "PlayStation 5",
        };

        _platformServiceMock
            .Setup(s => s.GetPlatformById(platformId))
            .ReturnsAsync(platformDto);

        // Act
        var result = await _controller.GetPlatformById(platformId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatform = Assert.IsType<PlatformDto>(okResult.Value);
        Assert.Equal(platformDto.Id, returnedPlatform.Id);
        Assert.Equal(platformDto.Type, returnedPlatform.Type);
    }

    [Fact]
    public async Task GetPlatformById_ShouldReturnNotFound_WhenPlatformDoesNotExist()
    {
        // Arrange
        var platformId = Guid.NewGuid();

        _platformServiceMock
            .Setup(s => s.GetPlatformById(platformId))
            .Returns(Task.FromResult<PlatformDto>(null));

        // Act
        var result = await _controller.GetPlatformById(platformId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPlatformById_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var platformId = Guid.NewGuid();

        _platformServiceMock
            .Setup(s => s.GetPlatformById(platformId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _controller.GetPlatformById(platformId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Database connection error", errorDict["Details"]);
    }

    [Fact]
    public async Task GetPlatformsByGameKey_ShouldReturnOk_WhenPlatformsExist()
    {
        // Arrange
        var gameKey = "test-game";
        var platforms = new List<PlatformDto>
    {
        new() { Id = Guid.NewGuid(), Type = "PlayStation 5" },
        new() { Id = Guid.NewGuid(), Type = "Xbox Series X" },
    };

        _platformServiceMock
            .Setup(s => s.GetPlatformsByGameKeyAsync(gameKey))
            .ReturnsAsync(platforms);

        // Act
        var result = await _controller.GetPlatformsByGameKey(gameKey);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPlatforms = Assert.IsAssignableFrom<IEnumerable<PlatformDto>>(okResult.Value);
        Assert.Equal(2, returnedPlatforms.Count());
        Assert.Contains(returnedPlatforms, p => p.Type == "PlayStation 5");
        Assert.Contains(returnedPlatforms, p => p.Type == "Xbox Series X");
    }

    [Fact]
    public async Task GetPlatformsByGameKey_ShouldReturnNotFound_WhenNoPlatformsExist()
    {
        // Arrange
        var gameKey = "game-without-platforms";

        _platformServiceMock
            .Setup(s => s.GetPlatformsByGameKeyAsync(gameKey))
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetPlatformsByGameKey(gameKey);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("No platforms found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetPlatformsByGameKey_ShouldReturnStatusCode500_WhenExceptionOccurs()
    {
        // Arrange
        var gameKey = "error-game";

        _platformServiceMock
            .Setup(s => s.GetPlatformsByGameKeyAsync(gameKey))
            .ThrowsAsync(new Exception("Game key cannot be empty"));

        // Act
        var result = await _controller.GetPlatformsByGameKey(gameKey);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Game key cannot be empty", errorDict["Details"]);
    }
}