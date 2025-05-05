using Gamestore.Entities;
using Gamestore.Services.Dto.PublishersDto;
using Gamestore.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers;

[Route("api/publishers")]
[ApiController]
public class PublishersController(IPublisherService publisherService, ILogger<PublishersController> logger) : ControllerBase
{
    private readonly IPublisherService _publisherService = publisherService;
    private readonly ILogger<PublishersController> _logger = logger;

    [HttpGet("ByName/{companyName}")]
    public async Task<ActionResult<Publisher>> GetPublisherByName(string companyName)
    {
        _logger.LogInformation("Getting publisher by Name: {PublisherName}", companyName);

        try
        {
            var publisher = await _publisherService.GetPublisherByCompanyNameAsync(companyName);
            if (publisher == null)
            {
                _logger.LogWarning("Publisher with name {PublisherName} not found", companyName);
                return NotFound($"Publisher with name '{companyName}' not found.");
            }

            _logger.LogInformation("Successfully found Publisher with name: {PublisherName}.", companyName);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting game by key: {companyName}", companyName);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("Games/{publisherName}")]
    public async Task<ActionResult<IEnumerable<Game>>> GetGamesByPublisherName(string publisherName)
    {
        _logger.LogInformation("Getting games by publisher name: {PublisherName}.", publisherName);

        try
        {
            var games = await _publisherService.GetGamesByPublisherNameAsync(publisherName);
            if (games == null)
            {
                _logger.LogWarning("Games with publisher name: {PublisherName} not found.", publisherName);
                return NotFound($"Games with publisher name: '{publisherName}' not found.");
            }

            _logger.LogInformation("Successfully found games with publisher name: {publisherName}", publisherName);
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting publisher by name: {publisherName}", publisherName);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Publisher>>> GetAllPublishers()
    {
        _logger.LogInformation("Getting all publishers");

        try
        {
            var games = await _publisherService.GetAllPublishersAsync();
            if (games == null || !games.Any())
            {
                _logger.LogWarning("No publishers found in the database");
                return NotFound("No publishers found.");
            }

            _logger.LogInformation("Successfully retrieved {Count} publishers", games.Count());
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all publishers");
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpGet("publisher/{key}")]
    public async Task<IActionResult> GetGamePublisherByGameKey(string key)
    {
        _logger.LogInformation("Getting publisher for game with key: {Key}", key);
        try
        {
            var publisher = await _publisherService.GetPublisherByGameKey(key);

            if (publisher == null)
            {
                _logger.LogWarning("Publisher for game with key: {Key} not found", key);
                return NotFound($"Publisher for game with key: '{key}' not found.");
            }

            _logger.LogInformation("Successfully found publisher for game with key: {Key}", key);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting publisher for game with key: {Key}", key);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpPost("add-publisher")]
    public async Task<IActionResult> CreatePublisher([FromBody] PublisherCreateDto publisherRequest)
    {
        _logger.LogInformation("Creating publisher with Name: {PublisherName}", publisherRequest.CompanyName);
        try
        {
            var publisher = await _publisherService.AddPublisherAsync(publisherRequest);
            _logger.LogInformation("Successfully created publisher with ID: {PublisherId}", publisher.CompanyName);
            return Ok(publisher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating publisher");
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePublisherById(Guid id)
    {
        _logger.LogInformation("Deleting publisher with ID: {Id}", id);
        try
        {
            var deletedPublisher = await _publisherService.DeletePublisherAsync(id);

            _logger.LogInformation("Successfully deleted publisher with ID: {Id}", id);
            return Ok(deletedPublisher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting publisher with ID: {Id}", id);
            return StatusCode(500, new { Message = "An error occurred.", Details = ex.Message });
        }
    }
}
