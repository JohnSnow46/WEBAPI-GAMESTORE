using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services.Dto;
using Gamestore.Services.IServices;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services;

public class PlatformService(IUnitOfWork unitOfWork, ILogger<PlatformService> logger) : IPlatformService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PlatformService> _logger = logger;

    public async Task<PlatformUpdateDto> UpdatePlatform(Guid id, PlatformUpdateDto platformRequest)
    {
        _logger.LogInformation("Starting update platform operation");
        try
        {
            // ValidatePlatformRequest(platformRequest);
            var platformEntity = await GetPlatformByIdAsync(id);
            _logger.LogInformation("Platform found with ID: {PlatformId}", platformEntity.Id);

            if (!string.Equals(platformEntity.Type, platformRequest.Type, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Platform type changed from '{OldType}' to '{NewType}' - validating uniqueness", platformEntity.Type, platformRequest.Type);
                await ValidatePlatformTypeUniqueness(platformRequest.Type ?? string.Empty);
            }

            platformEntity.Type = platformRequest.Type ?? string.Empty;
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Platform updated with ID: {PlatformId}", platformEntity.Id);

            return new PlatformUpdateDto
            {
                Id = platformEntity.Id,
                Type = platformEntity.Type,
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Platform with ID: {PlatformId} not found", platformRequest.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating platform");
            throw new InvalidOperationException($"Error updating platform: {ex.Message}", ex);
        }
    }

    public async Task<PlatformDto> CreatePlatform(PlatformRequestDto platformRequest)
    {
        _logger.LogInformation("Starting create platform operation");
        try
        {
            // ValidatePlatformRequest(platformRequest);
            _logger.LogInformation("Validating uniqueness for new platform type: {PlatformType}", platformRequest.Platform.Type);
            await ValidatePlatformTypeUniqueness(platformRequest.Platform.Type ?? string.Empty);

            Platform platformEntity = new()
            {
                Id = platformRequest.Platform.Id != Guid.Empty ? platformRequest.Platform.Id : Guid.NewGuid(),
                Type = platformRequest.Platform.Type ?? string.Empty,
            };

            await _unitOfWork.Platforms.AddAsync(platformEntity);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("New platform created with ID: {PlatformId}", platformEntity.Id);

            return MapToPlatformDto(platformEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating platform");
            throw new InvalidOperationException($"Error creating platform: {ex.Message}", ex);
        }
    }

    public async Task<Platform> DeletePlatformById(Guid id)
    {
        _logger.LogInformation("Starting delete platform operation for ID: {PlatformId}", id);

        try
        {
            ValidatePlatformId(id);

            var platformEntity = await GetPlatformByIdAsync(id);
            await ValidatePlatformCanBeDeleted(id);

            await _unitOfWork.Platforms.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Platform with ID: {PlatformId} deleted successfully", id);
            return platformEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting platform with ID: {PlatformId}", id);
            throw new InvalidOperationException($"Error deleting platform: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Platform>> GetAllPlatformsAsync()
    {
        _logger.LogInformation("Starting get all platforms operation");

        try
        {
            var platforms = await _unitOfWork.Platforms.GetAllAsync();
            var platformsList = platforms.ToList();

            _logger.LogInformation("Retrieved {Count} platforms from database", platformsList.Count);

            return platformsList.Select(MapToPlatform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all platforms");
            throw new InvalidOperationException($"Error retrieving all platforms: {ex.Message}", ex);
        }
    }

    public async Task<Platform> GetPlatformById(Guid id)
    {
        _logger.LogInformation("Starting get platform by ID operation for ID: {PlatformId}", id);

        try
        {
            ValidatePlatformId(id);

            var platform = await GetPlatformByIdAsync(id);
            _logger.LogInformation("Platform found with ID: {PlatformId}", id);

            return MapToPlatform(platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving platform with ID: {PlatformId}", id);
            throw new InvalidOperationException($"Error retrieving platform by ID: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Platform>> GetPlatformsByGameKeyAsync(string gameKey)
    {
        _logger.LogInformation("Starting get platforms by game key operation for key: {GameKey}", gameKey);

        try
        {
            ValidateGameKey(gameKey);
            var game = await GetGameByKeyAsync(gameKey);

            return await RetrievePlatformsByGameAsync(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving platforms for game with key: {GameKey}", gameKey);
            throw new InvalidOperationException($"Error retrieving platforms by game key: {ex.Message}", ex);
        }
    }

    private async Task ValidatePlatformTypeUniqueness(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ValidationException("Platform type cannot be empty");
        }

        var platforms = await _unitOfWork.Platforms.GetAllAsync();
        var exists = platforms.Any(p => string.Equals(p.Type, type, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            throw new ValidationException($"Platform with type '{type}' already exists");
        }
    }

    private async Task<bool> IsPlatformUsedByGames(Guid platformId)
    {
        var gamePlatforms = await _unitOfWork.GamePlatforms.GetByPlatformIdAsync(platformId);
        return gamePlatforms != null && gamePlatforms.Any();
    }

    private void ValidatePlatformId(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided platform ID is empty");
            throw new ArgumentException("Platform ID cannot be empty", nameof(id));
        }
    }

    private void ValidateGameKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Provided game key is null or empty");
            throw new ArgumentException("Game key cannot be null or empty", nameof(key));
        }
    }

    private async Task<Platform> GetPlatformByIdAsync(Guid id)
    {
        var platform = await _unitOfWork.Platforms.GetByIdAsync(id);

        if (platform == null)
        {
            _logger.LogWarning("Platform not found for ID: {PlatformId}", id);
            throw new KeyNotFoundException($"Platform with ID '{id}' not found");
        }

        return platform;
    }

    private async Task<Game> GetGameByKeyAsync(string key)
    {
        var game = await _unitOfWork.Games.GetKeyAsync(key);

        if (game == null)
        {
            _logger.LogWarning("Game not found for key: {GameKey}", key);
            throw new KeyNotFoundException($"Game with key '{key}' not found");
        }

        _logger.LogInformation("Game found with ID: {GameId} for key: {GameKey}", game.Id, key);

        return game;
    }

    private async Task ValidatePlatformCanBeDeleted(Guid platformId)
    {
        if (await IsPlatformUsedByGames(platformId))
        {
            _logger.LogWarning("Cannot delete platform with ID: {PlatformId} because it is used by games", platformId);
            throw new InvalidOperationException("Cannot delete a platform that is used by games. Please remove the platform from games first");
        }
    }

    private async Task<IEnumerable<Platform>> RetrievePlatformsByGameAsync(Game game)
    {
        _logger.LogInformation("Retrieving platforms for game with ID: {GameId}", game.Id);

        var gamePlatforms = await _unitOfWork.GamePlatforms.GetByGameIdAsync(game.Id);

        if (gamePlatforms == null || gamePlatforms.Count == 0)
        {
            _logger.LogInformation("No platforms found for game with ID: {GameId}", game.Id);
            throw new KeyNotFoundException($"No platforms found for game with key '{game.Key}'");
        }

        return await FetchAndMapPlatformsAsync(gamePlatforms);
    }

    private async Task<IEnumerable<Platform>> FetchAndMapPlatformsAsync(IEnumerable<GamePlatform> gamePlatforms)
    {
        var platformIds = gamePlatforms.Select(gp => gp.PlatformId).ToList();
        var platforms = await _unitOfWork.GamePlatforms.GetByIdsAsync(platformIds);

        _logger.LogInformation("Retrieved {Count} platforms for game relations", platforms.Count);

        return platforms;
    }

    private static PlatformDto MapToPlatformDto(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        return new PlatformDto
        {
            Id = platform.Id,
            Type = platform.Type,
        };
    }

    private static Platform MapToPlatform(Platform platformEntity)
    {
        ArgumentNullException.ThrowIfNull(platformEntity);
        return new Platform
        {
            Id = platformEntity.Id,
            Type = platformEntity.Type,
        };
    }
}