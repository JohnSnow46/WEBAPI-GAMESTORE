using System.Text.Json;
using Gamestore.Services.Dto.GamesDto;
using Gamestore.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

[ApiController]
[Route("api")]
public class GameController(IGameService gameService, ILogger<GameController> logger) : ControllerBase
{
    private readonly IGameService _gameService = gameService;
    private readonly ILogger<GameController> _logger = logger;

    [HttpPost("games/add-game")]
    public async Task<IActionResult> CreateGame([FromBody] GameRequestDto gameRequest)
    {
        // logger.LogInformation("Creating new game with Name: {GameName}", gameRequest.Game.Name);
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

    [HttpPut("games/update-game")]
    public async Task<IActionResult> UpdateGame([FromBody] JsonElement requestData)
    {
        try
        {
            _logger.LogInformation("Received raw update request: {RequestData}", requestData.ToString());
            if (!requestData.TryGetProperty("game", out var gameElement))
            {
                _logger.LogWarning("Invalid request format: missing 'game' property");
                return BadRequest(new { Message = "Invalid request format. Expected 'game' property." });
            }

            var gameUpdateDto = requestData.Deserialize<GameCreateUpdateDto>();
            if (gameUpdateDto == null || string.IsNullOrEmpty(gameUpdateDto.Game.Key))
            {
                _logger.LogWarning("Invalid game data or missing Key");
                return BadRequest(new { Message = "Invalid game data or missing Key." });
            }

            var key = gameUpdateDto.Game.Key;
            _logger.LogInformation(
                "Updating game with Key: {GameKey}, Title: {GameTitle}",
                key,
                gameUpdateDto.Game.Key);

            var updatedGame = await _gameService.UpdateGameAsync(key, gameUpdateDto);
            _logger.LogInformation("Successfully updated game with Key: {GameKey}", updatedGame.Key);
            return Ok(new { game = updatedGame });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for game update");
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Game not found");
            return NotFound(new { Message = "Game not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating game: {ErrorMessage}", ex.Message);
            return StatusCode(500, new
            {
                Message = "An error occurred while updating game.",
                Details = $"Error updating game: {ex.Message}",
            });
        }
    }

    [HttpGet("games/{key}")]
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

    [HttpGet("games/find/{id}")]
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

    [HttpDelete("games/{key}")]
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

    [HttpGet("games/")]
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

    [HttpPost("games/download-game-file/{gameId}")]
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

    [HttpGet("platforms/{id}/games")]
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
}