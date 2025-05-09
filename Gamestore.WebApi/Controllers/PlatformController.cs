﻿using System.Text.Json;
using Gamestore.Entities;
using Gamestore.Services.Dto;
using Gamestore.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

[Route("api/platforms")]
[ApiController]
public class PlatformController(IPlatformService platformService, ILogger<PlatformController> logger) : ControllerBase
{
    private readonly IPlatformService _platformService = platformService;
    private readonly ILogger<PlatformController> _logger = logger;

    [HttpPost("add-platform")]
    public async Task<IActionResult> CreateOrUpdatePlatform([FromBody] PlatformRequestDto platformRequest)
    {
        _logger.LogInformation("Creating or updating platform with ID: {PlatformId}, Name: {PlatformName}", platformRequest.Platform.Id, platformRequest.Platform.Type);

        try
        {
            var updatedPlatform = await _platformService.CreatePlatform(platformRequest);
            if (updatedPlatform == null)
            {
                _logger.LogWarning("Platform with ID: {PlatformId} not found for update", platformRequest.Platform.Id);
                return NotFound($"Platform with ID '{platformRequest.Platform.Id}' not found.");
            }

            _logger.LogInformation("Successfully processed platform with ID: {PlatformId}", updatedPlatform.Id);
            return Ok(updatedPlatform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or updating platform with ID: {PlatformId}", platformRequest.Platform.Id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpPut("update-platform")]
    public async Task<IActionResult> UpdatePlatform([FromBody] JsonElement requestData)
    {
        try
        {
            _logger.LogInformation("Received raw update request: {RequestData}", requestData.ToString());

            if (!requestData.TryGetProperty("platform", out var platformElement))
            {
                _logger.LogWarning("Invalid request format: missing 'genre' property");
                return BadRequest(new { Message = "Invalid request format. Expected 'genre' property." });
            }

            var platformUpdateDto = platformElement.Deserialize<PlatformUpdateDto>();
            if (platformUpdateDto == null || platformUpdateDto.Id == Guid.Empty)
            {
                _logger.LogWarning("Invalid genre data or missing ID");
                return BadRequest(new { Message = "Invalid genre data or missing ID." });
            }

            var id = platformUpdateDto.Id;

            var updatedGenre = await _platformService.UpdatePlatform(id, platformUpdateDto);
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
            return NotFound(new { Message = "Genre not found." });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("cycle"))
        {
            _logger.LogWarning(ex, "Cyclic reference detected in genre hierarchy");
            return BadRequest(new { Message = "Cyclic reference detected in genre hierarchy." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating genre: {ErrorMessage}", ex.Message);
            return StatusCode(500, new
            {
                Message = "An error occurred while updating genre.",
                Details = $"Error updating genre: {ex.Message}",
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlatformById(Guid id)
    {
        _logger.LogInformation("Deleting platform with ID: {PlatformId}", id);

        try
        {
            var deletedPlatform = await _platformService.DeletePlatformById(id);
            if (deletedPlatform == null)
            {
                _logger.LogWarning("Platform with ID: {PlatformId} not found for deletion", id);
                return NotFound($"Platform with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully deleted platform with ID: {PlatformId}", id);
            return Ok(deletedPlatform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting platform with ID: {PlatformId}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Platform>>> GetAllPlatforms()
    {
        _logger.LogInformation("Getting all platforms");

        try
        {
            var platforms = await _platformService.GetAllPlatformsAsync();
            _logger.LogInformation("Successfully retrieved {Count} platforms", platforms.Count());
            return Ok(platforms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all platforms");
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlatformById(Guid id)
    {
        _logger.LogInformation("Getting platform by ID: {PlatformId}", id);

        try
        {
            var platform = await _platformService.GetPlatformById(id);
            if (platform == null)
            {
                _logger.LogWarning("Platform with ID: {PlatformId} not found", id);
                return NotFound($"Platform with ID '{id}' not found.");
            }

            _logger.LogInformation("Successfully retrieved platform with ID: {PlatformId}", id);
            return Ok(platform);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting platform by ID: {PlatformId}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("{key}/platforms")]
    public async Task<IActionResult> GetPlatformsByGameKey(string key)
    {
        _logger.LogInformation("Getting platforms for game with key: {GameKey}", key);

        try
        {
            var platforms = await _platformService.GetPlatformsByGameKeyAsync(key);
            if (!platforms.Any())
            {
                _logger.LogWarning("No platforms found for game with key: {GameKey}", key);
                return NotFound($"No platforms found for game '{key}'.");
            }

            _logger.LogInformation("Successfully retrieved {Count} platforms for game with key: {GameKey}", platforms.Count(), key);
            return Ok(platforms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving platforms for game with key: {GameKey}", key);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }
}