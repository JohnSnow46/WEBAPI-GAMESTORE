using Gamestore.Services.Dto;
using Gamestore.Services.IServices;
using Gamestore.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace Gamestore.Tests.Controllers.Tests;

public class GenreControllerTests
{
    private readonly Mock<IGenreService> _genreServiceMock;
    private readonly GenreController _controller;

    public GenreControllerTests()
    {
        _genreServiceMock = new Mock<IGenreService>();
        var loggerMock = new Mock<ILogger<GenreController>>();
        _controller = new GenreController(_genreServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateGenre_ShouldReturnOk_WhenGenreIsCreatedOrUpdated()
    {
        var genreDto = new GenreDto { Id = Guid.NewGuid(), Name = "Action" };
        var request = new GenreRequestDto { Genre = genreDto };

        _genreServiceMock
            .Setup(s => s.CreateOrUpdateGenre(request))
            .ReturnsAsync(genreDto);

        var result = await _controller.CreateOrUpdateGenre(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedGenre = Assert.IsType<GenreDto>(okResult.Value);
        Assert.Equal(genreDto.Id, returnedGenre.Id);
        Assert.Equal(genreDto.Name, returnedGenre.Name);
    }

    [Fact]
    public async Task CreateOrUpdateGenre_ShouldReturnNotFound_WhenGenreNotFound()
    {
        // Arrange
        var genreDto = new GenreDto { Id = Guid.NewGuid(), Name = "NonExistentGenre" };
        var request = new GenreRequestDto { Genre = genreDto };

        _genreServiceMock
            .Setup(s => s.CreateOrUpdateGenre(request))
            .Returns(Task.FromResult<GenreDto>(null));

        // Act
        var result = await _controller.CreateOrUpdateGenre(request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateOrUpdateGenre_ShouldReturnBadRequest_WhenServiceThrowsException()
    {
        var genreDto = new GenreDto { Id = Guid.NewGuid(), Name = "Broken" };
        var request = new GenreRequestDto { Genre = genreDto };

        _genreServiceMock
            .Setup(s => s.CreateOrUpdateGenre(request))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.CreateOrUpdateGenre(request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnOk_WhenGenreExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new GenreDto { Id = id, Name = "Adventure" };

        _genreServiceMock
            .Setup(s => s.GetGenreById(id))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetGenreById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<GenreDto>(okResult.Value);
        Assert.Equal(expected.Id, returned.Id);
        Assert.Equal(expected.Name, returned.Name);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnNotFound_WhenGenreDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.GetGenreById(id))
            .Returns(Task.FromResult<GenreDto>(null));

        // Act
        var result = await _controller.GetGenreById(id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetGenreById_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrenge
        var id = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.GetGenreById(id))
            .ThrowsAsync(new KeyNotFoundException("Genre not found"));

        // Act
        var result = await _controller.GetGenreById(id);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Genre not found", errorDict["Details"]);
    }

    [Fact]
    public async Task GetGenresByParentId_ShouldReturnOk_WhenSubGenresExist()
    {
        var parentId = Guid.NewGuid();
        var subGenres = new List<GenreDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Indie RPG" },
        };

        _genreServiceMock
            .Setup(s => s.GetSubGenresAsync(parentId))
            .ReturnsAsync(subGenres);

        var result = await _controller.GetGenresByParentId(parentId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<GenreDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetGenresByParentId_ShouldReturnNotFound_WhenGenreDoesNotExist()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.GetSubGenresAsync(parentId))
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetGenresByParentId(parentId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("no subgenres found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetGenresByParentId_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        var parentId = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.GetSubGenresAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ArgumentException("Id cannot be empty"));

        var result = await _controller.GetGenresByParentId(parentId);

        var objectResult = Assert.IsType<ObjectResult>(result);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Id cannot be empty", errorDict["Details"]);
    }

    [Fact]
    public async Task DeleteGenreById_ShouldReturnOk_WhenGenreIsDeleted()
    {
        var id = Guid.NewGuid();
        var genre = new GenreDto { Id = id, Name = "Puzzle" };

        _genreServiceMock
            .Setup(s => s.DeleteGenreById(id))
            .ReturnsAsync(genre);

        var result = await _controller.DeleteGenreById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var deleted = Assert.IsType<GenreDto>(okResult.Value);
        Assert.Equal(genre.Id, deleted.Id);
        Assert.Equal(genre.Name, deleted.Name);
    }

    [Fact]
    public async Task DeleteGenreById_ShouldReturnNotFound_WhenGenreDoesNotExist()
    {
        var id = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.DeleteGenreById(id))
            .ReturnsAsync((GenreDto)null!);

        var result = await _controller.DeleteGenreById(id);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteGenreById_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        var id = Guid.NewGuid();

        _genreServiceMock
            .Setup(s => s.DeleteGenreById(id))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.DeleteGenreById(id);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Unexpected error", errorDict["Details"]);
    }

    [Fact]
    public async Task GetGenresByGameKey_ShouldReturnOk_WhenGenresExist()
    {
        var key = "test-game";
        var genres = new List<GenreDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Horror" },
        };

        _genreServiceMock
            .Setup(s => s.GetGenresByGameKeyAsync(key))
            .ReturnsAsync(genres);

        var result = await _controller.GetGenresByGameKey(key);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<GenreDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task GetGenresByGameKey_ShouldReturnNotFound_WhenNoGenresExist()
    {
        var key = "missing-game";

        _genreServiceMock
            .Setup(s => s.GetGenresByGameKeyAsync(key))
            .ReturnsAsync([]);

        var result = await _controller.GetGenresByGameKey(key);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("no genres found", notFoundResult.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetGenresByGameKey_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        var key = "test-error";

        _genreServiceMock
            .Setup(s => s.GetGenresByGameKeyAsync(key))
            .ThrowsAsync(new ArgumentException("Invalid key format"));

        var result = await _controller.GetGenresByGameKey(key);

        var objectResult = Assert.IsType<ObjectResult>(result);

        var json = JsonConvert.SerializeObject(objectResult.Value);
        var errorDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        Assert.Equal("An error occurred.", errorDict["Message"]);
        Assert.Equal("Invalid key format", errorDict["Details"]);
    }
}