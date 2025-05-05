using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services.Dto.GenresDto;
using Gamestore.Services.IServices;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services;

public class GenreService(IUnitOfWork unitOfWork, ILogger<GenreService> logger) : IGenreService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GenreService> _logger = logger;

    public async Task<GenreDto> CreateGenre(GenreRequestDto genreRequest)
    {
        _logger.LogInformation("Starting create genre operation for name: {GenreName}", genreRequest?.Genre?.Name);
        try
        {
            ValidateGenreRequest(genreRequest);

            _logger.LogInformation("Validating uniqueness for new genre name: {GenreName}", genreRequest.Genre.Name);
            await ValidateGenreNameUniqueness(genreRequest.Genre.Name ?? string.Empty);

            var genreEntity = CreateNewGenreEntity(genreRequest.Genre);

            if (genreRequest.Genre.ParentGenreId.HasValue)
            {
                _logger.LogInformation("Validating parent genre existence for parent ID: {ParentId}", genreRequest.Genre.ParentGenreId.Value);
                await ValidateParentGenreExists(genreRequest.Genre.ParentGenreId.Value);
            }

            _logger.LogInformation("Created new genre entity with ID: {GenreId}", genreEntity.Id);
            await _unitOfWork.Genres.AddAsync(genreEntity);
            _logger.LogInformation("Added new genre to repository with ID: {GenreId}", genreEntity.Id);

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully saved new genre with ID: {GenreId}", genreEntity.Id);
            return MapToGenreDto(genreEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating genre with name: {GenreName}", genreRequest?.Genre?.Name);
            throw new InvalidOperationException($"Error creating genre: {ex.Message}", ex);
        }
    }

    public async Task<GenreDto> UpdateGenre(Guid id, GenreUpdateDto genreRequest)
    {
        _logger.LogInformation("Starting update genre operation for ID: {GenreId}", id);
        try
        {
            // ValidateGenreRequest(genreRequest);
            var genreEntity = await GetGenreEntityAsync(id);
            if (genreEntity == null)
            {
                _logger.LogWarning("No existing genre found with ID: {GenreId}", id);
                throw new KeyNotFoundException($"Genre with ID {id} not found");
            }

            _logger.LogInformation("Found existing genre with ID: {GenreId}", genreEntity.Id);

            if (!string.Equals(genreEntity.Name, genreRequest.Name, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Genre name changed from '{OldName}' to '{NewName}' - validating uniqueness", genreEntity.Name, genreRequest.Name);
                await ValidateGenreNameUniqueness(genreRequest.Name ?? string.Empty);
            }

            if (genreRequest.ParentGenreId.HasValue)
            {
                await ValidateGenreHierarchy(id, genreRequest.ParentGenreId.Value);
            }

            UpdateGenreEntity(genreEntity, genreRequest);

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Successfully updated genre with ID: {GenreId}", genreEntity.Id);
            return MapToGenreDto(genreEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating genre with ID: {GenreId}", id);
            throw new InvalidOperationException($"Error updating genre: {ex.Message}", ex);
        }
    }

    public async Task<GenreGetAllDto> GetGenreById(Guid id)
    {
        _logger.LogInformation("Starting get genre by ID operation for ID: {GenreId}", id);

        try
        {
            var genre = await ValidateAndGetGenreById(id);
            _logger.LogInformation("Genre found with ID: {GenreId}", genre.Id);

            return new GenreGetAllDto
            {
                Id = genre.Id,
                Name = genre.Name,
                ParentGenreId = genre.ParentGenreId,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving genre with ID: {GenreId}", id);
            throw new InvalidOperationException($"Error retrieving genre by ID: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<GenreGetAllDto>> GetAllGenres()
    {
        _logger.LogInformation("Starting get all genres operation");
        try
        {
            var genres = await _unitOfWork.Genres.GetAllAsync();
            var genresList = genres.ToList();
            _logger.LogInformation("Retrieved {Count} genres from database", genresList.Count);
            return genresList.Select(MapToGenreGetAllDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all genres");
            throw new InvalidOperationException($"Error retrieving all genres: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<GenreDto>> GetSubGenresAsync(Guid id)
    {
        _logger.LogInformation("Starting get sub-genres operation for genre ID: {GenreId}", id);

        try
        {
            await ValidateAndGetGenreById(id);
            _logger.LogInformation("Parent genre exists with ID: {GenreId}", id);

            return await RetrieveSubGenresAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sub-genres for genre ID: {GenreId}", id);
            throw new InvalidOperationException($"Error retrieving sub-genres: {ex.Message}", ex);
        }
    }

    public async Task<GenreDto> DeleteGenreById(Guid id)
    {
        _logger.LogInformation("Starting delete genre operation for ID: {GenreId}", id);

        try
        {
            var genre = await ValidateAndGetGenreById(id);

            await ValidateGenreCanBeDeleted(id);

            _logger.LogInformation("Deleting genre with ID: {GenreId}", id);
            await _unitOfWork.Genres.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully deleted genre with ID: {GenreId}", id);

            return new GenreDto
            {
                Id = genre.Id,
                Name = genre.Name,
                ParentGenreId = genre.ParentGenreId,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting genre with ID: {GenreId}", id);
            throw new InvalidOperationException($"Error deleting genre: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<GenreDto>> GetGenresByGameKeyAsync(string gameKey)
    {
        _logger.LogInformation("Starting get genres by game key operation for key: {GameKey}", gameKey);

        try
        {
            ValidateGameKey(gameKey);

            var game = await GetGameByKeyAsync(gameKey);
            return await RetrieveGenresByGameAsync(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving genres for game with key: {GameKey}", gameKey);
            throw new InvalidOperationException($"Error retrieving genres by game key: {ex.Message}", ex);
        }
    }

    private async Task<Genre> GetGenreEntityAsync(Guid genreId)
    {
        _logger.LogInformation("Getting genre entity for ID: {GenreId}", genreId);

        var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);

        if (genre == null)
        {
            _logger.LogWarning("Genre not found for ID: {GenreId}", genreId);
            throw new KeyNotFoundException($"Genre with ID '{genreId}' not found");
        }

        return genre;
    }

    private static void UpdateGenreEntity(Genre genreEntity, GenreUpdateDto genre)
    {
        ArgumentNullException.ThrowIfNull(genreEntity);
        ArgumentNullException.ThrowIfNull(genre);

        if (string.IsNullOrWhiteSpace(genre.Name))
        {
            throw new ArgumentException("Genre name cannot be null or empty.", nameof(genre));
        }

        genreEntity.Name = genre.Name;
        genreEntity.ParentGenreId = genre.ParentGenreId;
    }

    private static Genre CreateNewGenreEntity(GenreDto genre)
    {
        ArgumentNullException.ThrowIfNull(genre);
        return string.IsNullOrWhiteSpace(genre.Name)
            ? throw new ArgumentException("Genre name cannot be null or empty.", nameof(genre))
            : new Genre
            {
                Id = Guid.NewGuid(),
                Name = genre.Name,
                ParentGenreId = genre.ParentGenreId,
            };
    }

    private static GenreDto MapToGenreDto(Genre genreEntity)
    {
        ArgumentNullException.ThrowIfNull(genreEntity);

        return new GenreDto
        {
            Id = genreEntity.Id,
            Name = genreEntity.Name,
            ParentGenreId = genreEntity.ParentGenreId,
        };
    }

    private static void ValidateGenreRequest(GenreRequestDto? genreRequest)
    {
        ArgumentNullException.ThrowIfNull(genreRequest);
        ArgumentNullException.ThrowIfNull(genreRequest.Genre);

        if (string.IsNullOrWhiteSpace(genreRequest.Genre.Name))
        {
            throw new ValidationException("Genre name cannot be empty");
        }

        if (genreRequest.Genre.Name.Length > 50)
        {
            throw new ValidationException("Genre name cannot exceed 50 characters");
        }
    }

    private async Task ValidateGenreNameUniqueness(string name)
    {
        _logger.LogInformation("Validating uniqueness for genre name: {GenreName}", name);

        var genres = await _unitOfWork.Genres.GetAllAsync();
        var exists = genres.Any(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            _logger.LogWarning("Genre with name '{GenreName}' already exists", name);
            throw new ValidationException($"Genre with name '{name}' already exists");
        }
    }

    private async Task ValidateGenreHierarchy(Guid genreId, Guid parentGenreId)
    {
        _logger.LogInformation("Validating genre hierarchy for genre ID: {GenreId} with parent ID: {ParentId}", genreId, parentGenreId);

        var parentExists = await _unitOfWork.Genres.GetByIdAsync(parentGenreId) != null;
        if (!parentExists)
        {
            _logger.LogWarning("Parent genre with ID: {ParentId} does not exist", parentGenreId);
            throw new ValidationException($"Parent genre with ID '{parentGenreId}' does not exist");
        }

        if (genreId == parentGenreId)
        {
            _logger.LogWarning("Genre cannot be its own parent - Genre ID: {GenreId}", genreId);
            throw new ValidationException("A genre cannot be its own parent");
        }

        var currentParentId = parentGenreId;
        var visitedIds = new HashSet<Guid> { genreId };

        _logger.LogInformation("Checking for circular references in genre hierarchy");

        while (currentParentId != Guid.Empty && !visitedIds.Contains(currentParentId))
        {
            visitedIds.Add(currentParentId);
            var parent = await _unitOfWork.Genres.GetByIdAsync(currentParentId);
            if (parent == null)
            {
                break;
            }

            currentParentId = parent.ParentGenreId ?? Guid.Empty;

            if (currentParentId == genreId)
            {
                _logger.LogWarning("Circular reference detected in genre hierarchy for genre ID: {GenreId}", genreId);
                throw new ValidationException("Circular reference detected in genre hierarchy");
            }
        }
    }

    private async Task<Genre> ValidateAndGetGenreById(Guid id)
    {
        ValidateGenreId(id);

        _logger.LogInformation("Validating genre exists with ID: {GenreId}", id);

        var genre = await _unitOfWork.Genres.GetByIdAsync(id);

        ValidateGenre(genre);

        return genre;
    }

    private void ValidateGenre(Genre genre)
    {
        if (genre == null)
        {
            _logger.LogWarning("Genre not found for ID: {GenreId}", genre.Id);
            throw new KeyNotFoundException($"Genre with ID '{genre.Id}' not found");
        }
    }

    private void ValidateGenreId(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided genre ID is empty");
            throw new ArgumentException("Genre ID cannot be empty", nameof(id));
        }
    }

    private async Task<bool> HasSubGenres(Guid genreId)
    {
        _logger.LogInformation("Checking if genre has sub-genres for ID: {GenreId}", genreId);

        var genres = await _unitOfWork.Genres.GetAllAsync();
        var hasSubGenres = genres.Any(g => g.ParentGenreId == genreId);

        _logger.LogInformation("Genre with ID: {GenreId} has sub-genres: {HasSubGenres}", genreId, hasSubGenres);

        return hasSubGenres;
    }

    private async Task<bool> IsGenreUsedByGames(Guid genreId)
    {
        _logger.LogInformation("Checking if genre is used by games for ID: {GenreId}", genreId);

        var gameGenres = await _unitOfWork.GameGenres.GetByGenreIdAsync(genreId);
        var isUsed = gameGenres != null && gameGenres.Any();

        _logger.LogInformation("Genre with ID: {GenreId} is used by games: {IsUsed}", genreId, isUsed);

        return isUsed;
    }

    private async Task ValidateGenreCanBeDeleted(Guid id)
    {
        _logger.LogInformation("Validating if genre can be deleted for ID: {GenreId}", id);

        _logger.LogInformation("Checking if genre has sub-genres for ID: {GenreId}", id);
        var hasSubGenres = await HasSubGenres(id);
        if (hasSubGenres)
        {
            _logger.LogWarning("Cannot delete genre with ID: {GenreId} because it has sub-genres", id);
            throw new InvalidOperationException("Cannot delete a genre that has sub-genres. Please delete the sub-genres first");
        }

        _logger.LogInformation("Checking if genre is used by games for ID: {GenreId}", id);
        var isUsedByGames = await IsGenreUsedByGames(id);
        if (isUsedByGames)
        {
            _logger.LogWarning("Cannot delete genre with ID: {GenreId} because it is used by games", id);
            throw new InvalidOperationException("Cannot delete a genre that is used by games. Please remove the genre from games first");
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

    private async Task<IEnumerable<GenreDto>> RetrieveGenresByGameAsync(Game game)
    {
        _logger.LogInformation("Retrieving genres for game with ID: {GameId}", game.Id);

        var gameGenres = await _unitOfWork.GameGenres.GetByGameIdAsync(game.Id);

        if (gameGenres == null || gameGenres.Count == 0)
        {
            _logger.LogInformation("No genres found for game with ID: {GameId}", game.Id);
            throw new KeyNotFoundException($"No genres found for game with ID '{game.Id}'");
        }

        _logger.LogInformation("Found {Count} game-genre relations for game with ID: {GameId}", gameGenres.Count, game.Id);

        return await FetchAndMapGenresAsync(gameGenres);
    }

    private async Task<IEnumerable<GenreDto>> FetchAndMapGenresAsync(IEnumerable<GameGenre> gameGenres)
    {
        var genreIds = gameGenres.Select(gg => gg.GenreId).ToList();
        var genres = await _unitOfWork.GameGenres.GetByIdsAsync(genreIds);
        _logger.LogInformation("Retrieved {Count} genres for game relations", genres.Count);
        return genres.Select(MapToGenreDto);
    }

    private async Task<IEnumerable<GenreDto>> RetrieveSubGenresAsync(Guid parentId)
    {
        _logger.LogInformation("Retrieving sub-genres for parent genre ID: {GenreId}", parentId);
        var allGenres = await _unitOfWork.Genres.GetAllAsync();
        var subGenres = allGenres.Where(g => g.ParentGenreId == parentId).ToList();
        if (subGenres.Count == 0)
        {
            _logger.LogInformation("No sub-genres found for parent genre ID: {GenreId}", parentId);
            throw new KeyNotFoundException($"No sub-genres found for genre with ID '{parentId}'");
        }

        _logger.LogInformation("Found {Count} sub-genres for parent genre ID: {GenreId}", subGenres.Count, parentId);
        return subGenres.Select(MapToGenreDto);
    }

    private async Task ValidateParentGenreExists(Guid parentId)
    {
        var parentGenre = await _unitOfWork.Genres.GetByIdAsync(parentId);
        if (parentGenre == null)
        {
            _logger.LogWarning("Parent genre with ID {ParentId} does not exist", parentId);
            throw new KeyNotFoundException($"Parent genre with ID {parentId} not found");
        }
    }

    private static GenreGetAllDto MapToGenreGetAllDto(Genre genreEntity)
    {
        ArgumentNullException.ThrowIfNull(genreEntity);
        return new GenreGetAllDto
        {
            Id = genreEntity.Id,
            Name = genreEntity.Name,
            ParentGenreId = genreEntity.ParentGenreId,
        };
    }
}