using Gamestore.Services.Dto.GenresDto;
using Gamestore.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Gamestore.WebApi.Controllers;

[ApiController]
[Route("api/genres")]
public class GenreController(IGenreService genreService, ILogger<GenreController> logger) : ControllerBase
{
    private readonly IGenreService _genreService = genreService;
    private readonly ILogger<GenreController> _logger = logger;

    [HttpPost("add-genre")]
    public async Task<IActionResult> CreateGenre([FromBody] GenreRequestDto genreRequest)
    {
        _logger.LogInformation("Creating new genre with Name: {GenreName}", genreRequest.Genre.Name);
        try
        {
            var newGenre = await _genreService.CreateGenre(genreRequest);
            _logger.LogInformation("Successfully created genre with ID: {GenreId}", newGenre.Id);
            return CreatedAtAction(nameof(GetGenreById), new { id = newGenre.Id }, new { genre = newGenre });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for genre creation");
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Referenced parent genre not found");
            return BadRequest(new { Message = "Referenced parent genre does not exist." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating genre");
            return StatusCode(500, new { Message = "An error occurred while creating genre.", Details = ex.Message });
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateGenre([FromBody] GenreUpdateDto genreRequest)
    {
        var id = genreRequest.Id;
        _logger.LogInformation("Updating genre with ID: {GenreId}", genreRequest.Name);
        try
        {
            var updatedGenre = await _genreService.UpdateGenre(id, genreRequest);
            _logger.LogInformation("Successfully updated genre with ID: {GenreId}", updatedGenre.Id);
            return Ok(new { genre = updatedGenre });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for genre update");
            return BadRequest(new { Message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Genre not found");
            return NotFound(new { Message = $"Genre with ID {genreRequest.Name} not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("cycle"))
        {
            _logger.LogWarning(ex, "Cyclic reference detected in genre hierarchy");
            return BadRequest(new { Message = "Cyclic reference detected in genre hierarchy." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating genre");
            return StatusCode(500, new { Message = "An error occurred while updating genre.", Details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGenreById(Guid id)
    {
        _logger.LogInformation("Getting genre by ID: {GenreId}", id);

        try
        {
            var genre = await _genreService.GetGenreById(id);
            if (genre == null)
            {
                _logger.LogWarning("Genre with ID: {GenreId} not found", id);
                return NotFound($"Genre with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved genre with ID: {GenreId}", id);
            return Ok(genre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting genre by ID: {GenreId}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGenres()
    {
        _logger.LogInformation("Getting all genres");
        try
        {
            var genres = await _genreService.GetAllGenres();
            _logger.LogInformation("Successfully retrieved all genres");
            return Ok(genres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all genres");
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("{id}/subgenres")]
    public async Task<IActionResult> GetGenresByParentId(Guid id)
    {
        _logger.LogInformation("Getting subgenres for parent genre ID: {ParentId}", id);

        try
        {
            var subGenres = await _genreService.GetSubGenresAsync(id);
            if (!subGenres.Any())
            {
                _logger.LogWarning("No subgenres found for parent genre ID: {ParentId}", id);
                return NotFound($"No subgenres found for genre ID '{id}'.");
            }

            _logger.LogInformation("Successfully retrieved {Count} subgenres for parent genre ID: {ParentId}", subGenres.Count(), id);
            return Ok(subGenres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subgenres for parent genre ID: {ParentId}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGenreById(Guid id)
    {
        _logger.LogInformation("Deleting genre with ID: {GenreId}", id);

        try
        {
            var genre = await _genreService.DeleteGenreById(id);
            if (genre == null)
            {
                _logger.LogWarning("Genre with ID: {GenreId} not found for deletion", id);
                return NotFound($"Genre with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully deleted genre with ID: {GenreId}", id);
            return Ok(genre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting genre with ID: {GenreId}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("{key}/genres")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> GetGenresByGameKey(string key)
    {
        _logger.LogInformation("Getting genres for game with key: {GameKey}", key);
        try
        {
            var genres = await _genreService.GetGenresByGameKeyAsync(key);
            if (!genres.Any())
            {
                _logger.LogWarning("No genres found for game with key: {GameKey}", key);
                return NotFound($"No genres found for game '{key}'.");
            }

            _logger.LogInformation("Successfully retrieved {Count} genres for game with key: {GameKey}", genres.Count(), key);
            return Ok(genres);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving genres for game with key: {GameKey}", key);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }
}