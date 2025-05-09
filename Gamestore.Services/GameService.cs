using System.ComponentModel.DataAnnotations;
using System.Text;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.IServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Gamestore.Services;

public class GameService(IUnitOfWork unitOfWork, ILogger<GameService> logger) : IGameService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<GameService> _logger = logger;

    public async Task<GameRequestDto> AddGameAsync(GameRequestDto gameRequest)
    {
        _logger.LogInformation("Starting add game operation for game: {GameName}", gameRequest.Game?.Name);

        try
        {
            if (gameRequest.Genres != null)
            {
                gameRequest.Genres = gameRequest.Genres
                    .Where(g => Guid.TryParse(g.ToString(), out _))
                    .ToList();
            }

            if (gameRequest.Platforms != null)
            {
                gameRequest.Platforms = gameRequest.Platforms
                    .Where(p => Guid.TryParse(p.ToString(), out _))
                    .ToList();
            }

            ValidateGameRequest(gameRequest);

            // Sprawdzenie unikalności klucza
            await ValidateGameKeyUniqueness(gameRequest.Game!.Key!);

            // Tworzenie nowego obiektu gry
            var newGame = new Game
            {
                Id = Guid.NewGuid(),
                Name = gameRequest.Game.Name,
                Key = gameRequest.Game.Key!,
                Description = gameRequest.Game.Description ?? string.Empty,
                Price = gameRequest.Game.Price,
                UnitInStock = gameRequest.Game.UnitInStock,
                Discontinued = gameRequest.Game.Discontinued,
                PublisherId = gameRequest.Publisher != Guid.Empty ? gameRequest.Publisher : null,
            };

            // Dodanie gry do repozytorium
            await _unitOfWork.Games.AddAsync(newGame);
            _logger.LogInformation("New game created with ID: {GameId}", newGame.Id);

            // Dodanie powiązań z platformami
            var validPlatformIds = gameRequest.Platforms?
                .Where(id => id != Guid.Empty)
                .ToList();

            if (validPlatformIds != null && validPlatformIds.Count > 0)
            {
                var gamePlatforms = validPlatformIds.Select(platformId => new GamePlatform
                {
                    GameId = newGame.Id,
                    PlatformId = platformId,
                }).ToList();

                await _unitOfWork.GamePlatforms.AddRangeAsync(gamePlatforms);
                _logger.LogInformation("Added {Count} platforms to game: {GameId}", gamePlatforms.Count, newGame.Id);
            }

            // Dodanie powiązań z gatunkami
            var validGenreIds = gameRequest.Genres?
                .Where(id => id != Guid.Empty)
                .ToList();

            if (validGenreIds != null && validGenreIds.Count > 0)
            {
                var gameGenres = validGenreIds.Select(genreId => new GameGenre
                {
                    GameId = newGame.Id,
                    GenreId = genreId,
                }).ToList();

                await _unitOfWork.GameGenres.AddRangeAsync(gameGenres);
                _logger.LogInformation("Added {Count} genres to game: {GameId}", gameGenres.Count, newGame.Id);
            }

            // Zapisanie wszystkich zmian w bazie danych
            await _unitOfWork.CompleteAsync();

            // Aktualizacja ID w zwracanym DTO
            gameRequest.Game.Id = newGame.Id;

            return gameRequest; // Zwrócenie tego samego DTO z zaktualizowanym ID
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding game: {GameName}", gameRequest.Game?.Name);
            throw new InvalidOperationException($"Error adding game: {ex.Message}", ex);
        }
    }

    public async Task<GameDtoUpdate> UpdateGameAsync(string key, GameCreateUpdateDto gameRequest)
    {
        _logger.LogInformation("Starting update game operation for key: {GameKey}", key);
        try
        {
            // Find the existing game
            var existingGame = await _unitOfWork.Games.GetKeyAsync(key);
            if (existingGame == null)
            {
                _logger.LogWarning("Game with key: {GameKey} not found for update", key);
                return null;
            }

            _logger.LogInformation("Game found. Updating game with ID: {GameId}", existingGame.Id);

            // Update basic game properties
            existingGame.Name = gameRequest.Game.Name;
            existingGame.Description = gameRequest.Game.Description ?? string.Empty;
            existingGame.Price = gameRequest.Game.Price;
            existingGame.UnitInStock = gameRequest.Game.UnitInStock;
            existingGame.Discontinued = gameRequest.Game.Discontinued;

            // Update publisher relation
            existingGame.PublisherId = gameRequest.Publisher != Guid.Empty ? gameRequest.Publisher : null;

            // Update game genres
            await UpdateGameGenres(existingGame.Id, gameRequest.Genres);

            // Update game platforms
            await UpdateGamePlatforms(existingGame.Id, gameRequest.Platforms);

            // Save all changes
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Game updated successfully with ID: {GameId}", existingGame.Id);

            // Return the updated game DTO
            return gameRequest.Game;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating game with key: {GameKey}", key);
            throw new InvalidOperationException($"Error updating game: {ex.Message}", ex);
        }
    }

    public async Task<GameDtoUpdate> GetGameByKey(string key)
    {
        _logger.LogInformation("Starting get game operation by key: {GameKey}", key);
        ValidateGameKey(key);
        try
        {
            var existingGame = await _unitOfWork.Games.GetKeyAsync(key);
            if (existingGame == null)
            {
                _logger.LogWarning("Game with key: {GameKey} not found", key);
                return null;
            }

            // Przygotuj informacje o grze
            var gameDetails = new GameDtoUpdate
            {
                Id = existingGame.Id,
                Name = existingGame.Name,
                Key = existingGame.Key,
                Description = existingGame.Description,
                Price = existingGame.Price,
                UnitInStock = existingGame.UnitInStock,
                Discontinued = existingGame.Discontinued,
            };

            // Pobierz nazwy gatunków z bezpiecznym sprawdzeniem null
            var genreNames = existingGame.GameGenres?
                .Where(gg => gg.Genre != null)
                .Select(gg => gg.Genre.Name)
                .ToList() ?? new List<string>();

            // Pobierz nazwy platform z bezpiecznym sprawdzeniem null
            var platformNames = existingGame.GamePlatforms?
                .Where(gp => gp.Platform != null)
                .Select(gp => gp.Platform.Type)
                .ToList() ?? new List<string>();

            // Pobierz nazwę wydawcy bezpośrednio z załadowanych danych
            string publisherName = existingGame.Publisher?.CompanyName ?? string.Empty;

            // Utwórz kompletne DTO
            var gameDto = new GameGetWithNames
            {
                // Game = gameDetails,
                Genres = genreNames,
                Platforms = platformNames,
                Publisher = publisherName,
            };

            _logger.LogInformation("Successfully retrieved game with ID: {GameId}", existingGame.Id);
            return gameDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game with key: {GameKey}", key);
            throw new InvalidOperationException($"Error retrieving game by key: {ex.Message}", ex);
        }
    }

    public async Task<GameDtoCreate> GetGameById(Guid id)
    {
        _logger.LogInformation("Starting get game operation by ID: {GameId}", id);
        ValidateGameId(id);

        try
        {
            var game = await _unitOfWork.Games.GetByIdAsync(id);

            ValidateGame(game);

            _logger.LogInformation("Game found with ID: {GameId}", game.Id);

            return MapToGameDto(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game with ID: {GameId}", id);
            throw new InvalidOperationException($"Error retrieving game by ID: {ex.Message}", ex);
        }
    }

    public async Task<Game> DeleteGameAsync(string key)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(key) || key == "undefined")
        {
            _logger.LogWarning("Invalid game key provided for deletion: '{Key}'", key);
            throw new ArgumentException("Game key cannot be null, empty, or 'undefined'", nameof(key));
        }

        _logger.LogInformation("Starting deletion process for game with key: {Key}", key);

        try
        {
            // Get the game entity
            var game = await _unitOfWork.Games.GetKeyAsync(key);
            if (game == null)
            {
                _logger.LogWarning("Game with key '{Key}' not found for deletion", key);
                return null;
            }

            // Remove related data first
            if (game.GameGenres?.Count > 0)
            {
                _logger.LogInformation("Removing {Count} genre associations for game ID: {GameId}", game.GameGenres.Count, game.Id);
                await _unitOfWork.GameGenres.RemoveRangeAsync(game.GameGenres);
            }

            if (game.GamePlatforms?.Count > 0)
            {
                _logger.LogInformation("Removing {Count} platform associations for game ID: {GameId}", game.GamePlatforms.Count, game.Id);
                await _unitOfWork.GamePlatforms.RemoveRangeAsync(game.GamePlatforms);
            }

            // Delete the game itself
            _logger.LogInformation("Deleting game with ID: {GameId}", game.Id);
            await _unitOfWork.Games.DeleteGameByKey(game);

            // Save changes
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully deleted game with key: {Key}, ID: {Id}", key, game.Id);

            return game;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting game with key: {Key}", key);
            throw new InvalidOperationException($"Failed to delete game with key '{key}'", ex);
        }
    }

    public async Task<IEnumerable<GameDtoCreate>> GetAllGames()
    {
        _logger.LogInformation("Starting get all games operation");

        try
        {
            var games = await _unitOfWork.Games.GetAllAsync();

            var gameList = games.ToList();
            _logger.LogInformation("Retrieved {Count} games from database", gameList.Count);

            return gameList.Select(MapToGameDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all games");
            throw new InvalidOperationException($"Error retrieving all games: {ex.Message}", ex);
        }
    }

    public async Task<string> CreateGameFileAsync(Guid gameId)
    {
        _logger.LogInformation("Starting create game file operation for game ID: {GameId}", gameId);

        try
        {
            ValidateGameId(gameId);

            var game = await _unitOfWork.Games.GetByIdAsync(gameId);

            ValidateGame(game);

            var serializedGame = JsonConvert.SerializeObject(game);

            var sanitizedName = SanitizeFileName(game.Name);
            var uniqueFileName = $"{sanitizedName}_{game.Id}.txt";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "GameFiles", uniqueFileName);

            _logger.LogInformation("Preparing to save file for game ID: {GameId} at path: {FilePath}", game.Id, filePath);

            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                _logger.LogInformation("Created directory for game files at: {DirectoryPath}", directoryPath);
            }

            await File.WriteAllTextAsync(filePath, serializedGame);

            _logger.LogInformation("Game file for game ID: {GameId} successfully written", game.Id);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game file for game ID: {GameId}", gameId);
            throw new InvalidOperationException($"Error creating game file: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<GameDtoCreate>> GetGamesByPlatformAsync(Guid platformId)
    {
        _logger.LogInformation("Starting get games by platform operation for platform ID: {PlatformId}", platformId);
        ValidatePlatformId(platformId);

        try
        {
            return await RetrieveGamesByPlatformAsync(platformId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for platform ID: {PlatformId}", platformId);
            throw new InvalidOperationException($"Error retrieving games by platform: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<GameDtoCreate>> GetGamesByGenreAsync(Guid genreId)
    {
        _logger.LogInformation("Starting get games by genre operation for genre ID: {GenreId}", genreId);
        ValidateGenreId(genreId);

        try
        {
            return await RetrieveGamesByGenreAsync(genreId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for genre ID: {GenreId}", genreId);
            throw new InvalidOperationException($"Error retrieving games by genre: {ex.Message}", ex);
        }
    }

    public async Task<int> GetTotalGamesCountAsync()
    {
        _logger.LogInformation("Starting get total games count operation");

        try
        {
            var count = await _unitOfWork.Games.CountAsync();
            _logger.LogInformation("Total games count: {GameCount}", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total games count");
            throw new InvalidOperationException($"Error getting total games count: {ex.Message}", ex);
        }
    }

    public static async Task<Game> AddOrUpdateGameAsync()
    {
        return await Task.FromResult<Game>(null);
    }

    private static void ValidateGameRequest(GameRequestDto gameRequest)
    {
        if (gameRequest == null)
        {
            throw new ArgumentNullException(nameof(gameRequest), "Game request cannot be null");
        }

        if (gameRequest.Game == null)
        {
            throw new ArgumentException("Game data is required", nameof(gameRequest));
        }

        if (string.IsNullOrWhiteSpace(gameRequest.Game.Name))
        {
            throw new ArgumentException("Game name is required", nameof(gameRequest));
        }

        if (string.IsNullOrWhiteSpace(gameRequest.Game.Key))
        {
            // Automatyczne generowanie klucza na podstawie nazwy
            gameRequest.Game.Key = GenerateKeyFromName(gameRequest.Game.Name);
        }

        if (gameRequest.Game.Price < 0)
        {
            throw new ArgumentException("Price cannot be negative", nameof(gameRequest));
        }

        if (gameRequest.Game.UnitInStock < 0)
        {
            throw new ArgumentException("Units in stock cannot be negative", nameof(gameRequest));
        }
    }

    private static string GenerateKeyFromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Guid.NewGuid().ToString("N")[..10];
        }

        // Zamień na małe litery
        var lower = name.ToLowerInvariant();

        // Buduj nowy ciąg znaków znak po znaku
        var builder = new StringBuilder(capacity: lower.Length);
        bool previousWasWhitespace = false;

        foreach (char c in lower)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append('-');
                    previousWasWhitespace = true;
                }
            }
            else if (char.IsLetterOrDigit(c) || c == '-')
            {
                builder.Append(c);
                previousWasWhitespace = false;
            }

            // znaki specjalne są ignorowane
        }

        // Ogranicz długość do 100 znaków
        string key = builder.ToString();
        if (key.Length > 100)
        {
            key = key[..100];
        }

        // Jeśli pusty, użyj fragmentu GUID
        if (string.IsNullOrWhiteSpace(key))
        {
            key = Guid.NewGuid().ToString("N")[..10];
        }

        return key;
    }

    private async Task ValidateGameKeyUniqueness(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ValidationException("Game key cannot be empty");
        }

        var game = await _unitOfWork.Games.GetKeyAsync(key);
        if (game != null)
        {
            throw new ValidationException($"Game with key '{key}' already exists");
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "unnamed";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        if (sanitized.Length > 50)
        {
            sanitized = sanitized[..50];
        }

        return sanitized;
    }

    private static GameDtoCreate MapToGameDto(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        return new GameDtoCreate
        {
            Id = game.Id,
            Description = game.Description,
            Key = game.Key,
            Name = game.Name,
            Price = game.Price,
            UnitInStock = game.UnitInStock,
            Discontinued = game.Discontinued,
        };
    }

    private void ValidateGameKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Provided game key is null or empty");
            throw new ArgumentException("Game key cannot be null or empty", nameof(key));
        }
    }

    private void ValidateGameId(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided game ID is empty");
            throw new ArgumentException("Game ID cannot be empty", nameof(id));
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

    private void ValidatePlatformId(Guid platformId)
    {
        if (platformId == Guid.Empty)
        {
            _logger.LogWarning("Provided platform ID is empty");
            throw new ArgumentException("Platform ID cannot be empty", nameof(platformId));
        }
    }

    private async Task<IEnumerable<GameDtoCreate>> RetrieveGamesByPlatformAsync(Guid platformId)
    {
        var gamePlatforms = await GetGamePlatformRelationsAsync(platformId);
        return await FetchAndMapGamesAsync(gamePlatforms);
    }

    private async Task<IEnumerable<GameDtoCreate>> RetrieveGamesByGenreAsync(Guid genreId)
    {
        var gameGenres = await GetGameGenreRelationsAsync(genreId);
        return await FetchAndMapGamesForGenreAsync(gameGenres);
    }

    private async Task<IEnumerable<GamePlatform>> GetGamePlatformRelationsAsync(Guid platformId)
    {
        var gamePlatforms = await _unitOfWork.GamePlatforms.GetByPlatformIdAsync(platformId);
        _logger.LogInformation("Found {Count} game-platform relations for platform ID: {PlatformId}", gamePlatforms.Count(), platformId);

        return gamePlatforms;
    }

    private async Task<IEnumerable<GameGenre>> GetGameGenreRelationsAsync(Guid genreId)
    {
        var gameGenres = await _unitOfWork.GameGenres.GetByGenreIdAsync(genreId);

        _logger.LogInformation("Found {Count} game-genre relations for genre ID: {GenreId}", gameGenres.Count(), genreId);

        return gameGenres;
    }

    private async Task<IEnumerable<GameDtoCreate>> FetchAndMapGamesAsync(IEnumerable<GamePlatform> gamePlatforms)
    {
        var gameIds = gamePlatforms.Select(gp => gp.GameId).ToList();
        var games = await _unitOfWork.Games.GetByIdsAsync(gameIds);

        _logger.LogInformation("Retrieved {Count} games for platform relations", games.Count());

        return games.Select(MapToGameDto);
    }

    private async Task<IEnumerable<GameDtoCreate>> FetchAndMapGamesForGenreAsync(IEnumerable<GameGenre> gameGenres)
    {
        var gameIds = gameGenres.Select(gg => gg.GameId).ToList();
        var games = await _unitOfWork.Games.GetByIdsAsync(gameIds);

        _logger.LogInformation("Retrieved {Count} games for genre relations", games.Count());

        return games.Select(MapToGameDto);
    }

    private static void ValidateGame(Game? game) => ArgumentNullException.ThrowIfNull(game);

    // Helper method to update game genres
    private async Task UpdateGameGenres(Guid gameId, List<Guid>? genreIds)
    {
        // Remove existing game-genre relationships
        var existingGenres = await _unitOfWork.GameGenres.GetByGameIdAsync(gameId);
        if (existingGenres.Count != 0)
        {
            await _unitOfWork.GameGenres.RemoveRangeAsync(existingGenres);
        }

        // Add new game-genre relationships if any
        if (genreIds != null && genreIds.Count != 0)
        {
            var validGenreIds = genreIds.Where(id => id != Guid.Empty).ToList();
            if (validGenreIds.Count != 0)
            {
                var gameGenres = validGenreIds.Select(genreId => new GameGenre
                {
                    GameId = gameId,
                    GenreId = genreId,
                }).ToList();

                await _unitOfWork.GameGenres.AddRangeAsync(gameGenres);
                _logger.LogInformation("Updated {Count} genres for game: {GameId}", gameGenres.Count, gameId);
            }
        }
    }

    // Helper method to update game platforms
    private async Task UpdateGamePlatforms(Guid gameId, List<Guid>? platformIds)
    {
        // Remove existing game-platform relationships
        var existingPlatforms = await _unitOfWork.GamePlatforms.GetByGameIdAsync(gameId);
        if (existingPlatforms.Count != 0)
        {
            await _unitOfWork.GamePlatforms.RemoveRangeAsync(existingPlatforms);
        }

        // Add new game-platform relationships if any
        if (platformIds != null && platformIds.Count != 0)
        {
            var validPlatformIds = platformIds.Where(id => id != Guid.Empty).ToList();
            if (validPlatformIds.Count != 0)
            {
                var gamePlatforms = validPlatformIds.Select(platformId => new GamePlatform
                {
                    GameId = gameId,
                    PlatformId = platformId,
                }).ToList();

                await _unitOfWork.GamePlatforms.AddRangeAsync(gamePlatforms);
                _logger.LogInformation("Updated {Count} platforms for game: {GameId}", gamePlatforms.Count, gameId);
            }
        }
    }
}