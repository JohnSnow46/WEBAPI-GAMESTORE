using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services;
using Moq;

namespace Gamestore.Tests.Services.Tests;
public class RelationManagerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GameRelationManager _relationManager;

    public RelationManagerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _relationManager = new GameRelationManager(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ManageGamePlatforms_ShouldReplaceExistingPlatforms()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId };
        var existingPlatforms = new List<GamePlatform>
        {
            new() { GameId = gameId, PlatformId = Guid.NewGuid() },
        };
        var newPlatformIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _unitOfWorkMock.Setup(u => u.GamePlatforms.GetByGameIdAsync(gameId))
            .ReturnsAsync(existingPlatforms);
        _unitOfWorkMock.Setup(u => u.GamePlatforms.RemoveRangeAsync(existingPlatforms))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.GamePlatforms.AddRangeAsync(It.IsAny<List<GamePlatform>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _relationManager.ManageGamePlatforms(game, newPlatformIds);

        // Assert
        _unitOfWorkMock.Verify(u => u.GamePlatforms.RemoveRangeAsync(existingPlatforms), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.GamePlatforms.AddRangeAsync(It.Is<List<GamePlatform>>(list =>
            list.Count == 2 &&
            list.All(p => p.GameId == gameId && newPlatformIds.Contains(p.PlatformId)))),
            Times.Once);
    }

    [Fact]
    public async Task ManageGameGenres_ShouldReplaceExistingGenres()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game { Id = gameId };
        var existingGenres = new List<GameGenre>
        {
            new() { GameId = gameId, GenreId = Guid.NewGuid() },
        };
        var newGenreIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _unitOfWorkMock.Setup(u => u.GameGenres.GetByGameIdAsync(gameId))
            .ReturnsAsync(existingGenres);
        _unitOfWorkMock.Setup(u => u.GameGenres.RemoveRangeAsync(existingGenres))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.GameGenres.AddRangeAsync(It.IsAny<List<GameGenre>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _relationManager.ManageGameGenres(game, newGenreIds);

        // Assert
        _unitOfWorkMock.Verify(u => u.GameGenres.RemoveRangeAsync(existingGenres), Times.Once);
        _unitOfWorkMock.Verify(
            u => u.GameGenres.AddRangeAsync(It.Is<List<GameGenre>>(list =>
            list.Count == 2 &&
            list.All(g => g.GameId == gameId && newGenreIds.Contains(g.GenreId)))),
            Times.Once);
    }
}
