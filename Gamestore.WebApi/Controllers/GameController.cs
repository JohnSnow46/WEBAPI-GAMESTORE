using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

[ApiController]
[Route("api/games")]
public class GameController(IGameService gameService, ILogger<GameController> logger) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly ILogger<GameController> _logger = logger;

    [HttpPost("add-game")]
    public async Task<IActionResult> CreateGame([FromBody] GameRequestDto gameRequest)
    {
        _logger.LogInformation("Creating new game with Name: {GameName}", gameRequest.Game.Name);
        try
        {
#pragma warning disable IDE0305 // Simplify collection initialization
            // Filter out invalid GUIDs before processing
            if (gameRequest.Genres != null)
            {
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
                gameRequest.Genres = gameRequest.Genres
                    .Where(g => g != null && Guid.TryParse(g.ToString(), out _))
                    .ToList();
            }

            if (gameRequest.Platforms != null)
            {
                gameRequest.Platforms = gameRequest.Platforms
                    .Where(p => p != null && Guid.TryParse(p.ToString(), out _))
                    .ToList();
#pragma warning restore IDE0305 // Simplify collection initialization
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            }

            var game = await _gameService.AddGameAsync(gameRequest);
            if (game == null)
            {
                _logger.LogWarning("Failed to create game with Name: {GameName}", gameRequest.Game.Name);
                return StatusCode(500, new { Message = "Failed to create the game." });
            }

            _logger.LogInformation("Successfully created game with ID: {GameId}", game.Game.Id);
            return CreatedAtAction(nameof(GetGameById), new { id = game.Game.Id }, gameRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game");
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpPut("update-game/{key}")]
    public async Task<IActionResult> UpdateGame(string key, [FromBody] GameCreateUpdateDto gameRequest)
    {
        if (key != gameRequest.Game.Key)
        {
            return BadRequest("Key in URL does not match key in request body");
        }

        try
        {
            var updatedGame = await _gameService.UpdateGameAsync(key, gameRequest);
            return updatedGame == null ? NotFound($"Game with key '{key}' not found") : Ok(updatedGame);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game with key: {GameKey}", key);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetGameByKey(string key)
    {
        _logger.LogInformation("Getting game by key: {Key}", key);

        try
        {
            var game = await _gameService.GetGameByKey(key);
            if (game == null)
            {
                _logger.LogWarning("Game with key: {Key} not found", key);
                return NotFound($"Game with key: '{key}' not found.");
            }

            _logger.LogInformation("Successfully found the game with key: {Key}", key);
            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game by key: {Key}", key);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("find/{id}")]
    public async Task<IActionResult> GetGameById(Guid id)
    {
        _logger.LogInformation("Getting game by id: {id}", id);

        try
        {
            var game = await _gameService.GetGameById(id);

            if (game == null)
            {
                _logger.LogWarning("Game with id: {id} not found", id);
                return NotFound($"Game with id: '{id}' not found.");
            }

            _logger.LogInformation("Successfully found the game with id: {id}", id);
            return Ok(game);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game by id: {id}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteGameByKey(string key)
    {
        _logger.LogInformation("Deleting game by key: {Key}", key);

        // Sprawdź czy klucz nie jest pusty lub "undefined"
        if (string.IsNullOrWhiteSpace(key) || key == "undefined")
        {
            _logger.LogWarning("Invalid key provided for deletion: '{Key}'", key);
            return BadRequest(new
            {
                Message = "Invalid game key provided",
                Details = "Game key cannot be empty or 'undefined'",
            });
        }

        try
        {
            var game = await _gameService.DeleteGameAsync(key);
            _logger.LogInformation("Successfully deleted game with key: {Key}", key);
            return Ok(game);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Game with key: {Key} not found for deletion", key);
            return NotFound(new
            {
                Message = "Game not found",
                Details = ex.Message,
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error deleting game by key: {Key}", key);
            return StatusCode(500, new
            {
                Message = "An error occurred while deleting the game.",
                Details = ex.Message,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game by key: {Key}", key);
            return StatusCode(500, new
            {
                Message = "An error occurred.",
                Details = ex.Message,
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        _logger.LogInformation("Getting all games");

        try
        {
            var games = await _gameService.GetAllGames();
            if (games == null || !games.Any())
            {
                _logger.LogWarning("No games found in the database");
                return NotFound("No games found.");
            }

            _logger.LogInformation("Successfully retrieved {Count} games", games.Count());
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all games");
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpPost("download-game-file/{gameId}")]
    public async Task<IActionResult> DownloadGameFile(Guid gameId)
    {
        _logger.LogInformation("Creating game file for game with ID: {GameId}", gameId);

        try
        {
            var filePath = await _gameService.CreateGameFileAsync(gameId);
            _logger.LogInformation("Successfully created game file at: {FilePath} for game ID: {GameId}", filePath, gameId);
            return Ok(new { FilePath = filePath });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Game not found for file creation. Game ID: {GameId}", gameId);
            return NotFound(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game file for game ID: {GameId}", gameId);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("get-games-by-platform-id/{id}")]
    public async Task<IActionResult> GetGamesByPlatformId(Guid id)
    {
        _logger.LogInformation("Getting games by platform ID: {PlatformId}", id);

        try
        {
            var platform = await _gameService.GetGamesByPlatformAsync(id);
            if (platform == null)
            {
                _logger.LogWarning("Platform with ID: {PlatformId} not found", id);
                return NotFound($"Platform with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved games for platform ID: {PlatformId}", id);
            return Ok(platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for platform ID: {PlatformId}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("get-games-by-genre-id/{id}")]
    public async Task<IActionResult> GetGamesByGenreId(Guid id)
    {
        _logger.LogInformation("Getting games by genre ID: {GenreId}", id);

        try
        {
            var genre = await _gameService.GetGamesByGenreAsync(id);
            if (genre == null)
            {
                _logger.LogWarning("Genre with ID: {GenreId} not found", id);
                return NotFound($"Genre with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved games for genre ID: {GenreId}", id);
            return Ok(genre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for genre ID: {GenreId}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }
}