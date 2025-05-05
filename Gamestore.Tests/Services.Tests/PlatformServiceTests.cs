using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services;
using Gamestore.Services.Dto;
using Microsoft.Extensions.Logging;
using Moq;

namespace Gamestore.Tests.Services.Tests;
public class PlatformServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly PlatformService _platformService;

    public PlatformServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<PlatformService>>();
        _platformService = new PlatformService(_unitOfWorkMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdatePlatform_ShouldUpdatePlatform_WhenPlatformExists()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platformRequest = new PlatfromRequestDto
        {
            Platform = new PlatformDto { Id = platformId, Type = "Updated Type" },
        };

        var existingPlatform = new Platform { Id = platformId, Type = "Old Type" };

        _unitOfWorkMock.Setup(u => u.Platforms.GetByIdAsync(platformId)).ReturnsAsync(existingPlatform);

        // Act
        var result = await _platformService.CreateOrUpdatePlatform(platformRequest);

        // Assert
        Assert.Equal(platformId, result.Id);
        Assert.Equal("Updated Type", result.Type);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllPlatformsAsync_ShouldReturnPlatformDtos_WhenPlatformsExist()
    {
        // Arrange
        var platforms = new List<Platform>
    {
        new() { Id = Guid.NewGuid(), Type = "PC" },
        new() { Id = Guid.NewGuid(), Type = "PlayStation" },
    };

        _unitOfWorkMock.Setup(u => u.Platforms.GetAllAsync()).ReturnsAsync(platforms);

        // Act
        var result = await _platformService.GetAllPlatformsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetPlatformById_ShouldReturnPlatformDto_WhenPlatformExists()
    {
        // Arrange
        var platformId = Guid.NewGuid();
        var platform = new Platform { Id = platformId, Type = "PC" };

        _unitOfWorkMock.Setup(u => u.Platforms.GetByIdAsync(platformId)).ReturnsAsync(platform);

        // Act
        var result = await _platformService.GetPlatformById(platformId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(platformId, result.Id);
        Assert.Equal("PC", result.Type);
    }
}