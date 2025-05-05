using System.ComponentModel.DataAnnotations;
using Gamestore.Data.Repository.IRepository;
using Gamestore.Entities;
using Gamestore.Services.Dto.PublishersDto;
using Gamestore.Services.IServices;
using Microsoft.Extensions.Logging;

namespace Gamestore.Services;

public class PublisherService(IUnitOfWork unitOfWork, ILogger<PublisherService> logger) : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PublisherService> _logger = logger;

    public async Task<IEnumerable<Publisher>> GetAllPublishersAsync()
    {
        _logger.LogInformation("Starting get all publishers operation");

        try
        {
            var publishers = await _unitOfWork.Publishers.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} publishers from database", publishers.Count());
            return publishers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all publishers");
            throw new InvalidOperationException($"Error retrieving all publishers: {ex.Message}", ex);
        }
    }

    public async Task<Publisher?> GetPublisherByIdAsync(Guid id)
    {
        _logger.LogInformation("Starting get publisher operation by ID: {PublisherId}", id);
        ValidatePublisherId(id);

        try
        {
            var publisher = await _unitOfWork.Publishers.GetByIdAsync(id);
            LogPublisherRetrievalResult(publisher, id);
            return publisher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publisher with ID: {PublisherId}", id);
            throw new InvalidOperationException($"Error retrieving publisher by ID: {ex.Message}", ex);
        }
    }

    public async Task<Publisher?> GetPublisherByCompanyNameAsync(string companyName)
    {
        _logger.LogInformation("Starting get publisher operation by company name: {CompanyName}", companyName);
        ValidateCompanyName(companyName);

        try
        {
            var publisher = await _unitOfWork.Publishers.GetByCompanyNameAsync(companyName);
            LogPublisherNameRetrievalResult(publisher, companyName);
            return publisher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publisher with company name: {CompanyName}", companyName);
            throw new InvalidOperationException($"Error retrieving publisher by company name: {ex.Message}", ex);
        }
    }

    public async Task<PublisherCreateDto> AddPublisherAsync(PublisherCreateDto publisher)
    {
        _logger.LogInformation("Starting add publisher operation");
        try
        {
            // Convert DTO to entity
            var publisherEntity = new Publisher
            {
                CompanyName = publisher.CompanyName,
                Description = publisher.Description,
                HomePage = publisher.HomePage,
            };

            // Add the entity to repository
            await _unitOfWork.Publishers.AddAsync(publisherEntity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Publisher added successfully with Name: {PublisherName}", publisherEntity.CompanyName);

            // Return the DTO
            return publisher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding publisher");
            throw new InvalidOperationException($"Error adding publisher: {ex.Message}", ex);
        }
    }

    public async Task<Publisher> CreateOrUpdatePublisherAsync(Publisher publisher)
    {
        _logger.LogInformation("Starting create or update publisher operation");

        try
        {
            ValidatePublisherEntity(publisher);

            return publisher.Id == Guid.Empty ? await CreateNewPublisher(publisher) : await UpdatePublisherRecord(publisher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating or updating publisher");
            throw new InvalidOperationException($"Error creating or updating publisher: {ex.Message}", ex);
        }
    }

    public async Task<PublisherDto> DeletePublisherAsync(Guid id)
    {
        _logger.LogInformation("Starting delete publisher operation for ID: {PublisherId}", id);
        ValidatePublisherId(id);

        try
        {
            var publisher = await ValidatePublisherExists(id);
            var publisherDto = MapToPublisherDto(publisher);

            await DeletePublisherEntity(id);

            _logger.LogInformation("Publisher with ID: {PublisherId} deleted successfully", id);
            return publisherDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting publisher with ID: {PublisherId}", id);
            throw new InvalidOperationException($"Error deleting publisher: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Game>> GetGamesByPublisherNameAsync(string publisherName)
    {
        _logger.LogInformation("Starting get games by publisher operation: {PublisherName}", publisherName);
        ValidateCompanyName(publisherName);

        try
        {
            var publisher = await FindPublisherByName(publisherName);
            var games = await FindGamesForPublisher(publisher.Id);

            _logger.LogInformation("Found {Count} games for publisher: {PublisherName}", games.Count, publisherName);
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving games for publisher name: {PublisherName}", publisherName);
            throw new InvalidOperationException($"Error retrieving games by publisher name: {ex.Message}", ex);
        }
    }

    public async Task<Publisher> GetPublisherByGameKey(string gameKey)
    {
        _logger.LogInformation("Starting get publisher by game key operation: {GameKey}", gameKey);
        ValidateGameKey(gameKey);

        try
        {
            var game = await FindGameByKey(gameKey);
            ValidateGameHasPublisher(game);
            var publisher = await FindPublisherById(game.PublisherId!.Value);

            _logger.LogInformation("Publisher found for game key: {GameKey}", gameKey);
            return publisher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publisher for game key: {GameKey}", gameKey);
            throw new InvalidOperationException($"Error retrieving publisher by game key: {ex.Message}", ex);
        }
    }

    private async Task<Publisher> CreateNewPublisher(Publisher publisher)
    {
        publisher = EnsurePublisherHasId(publisher);
        _logger.LogInformation("Creating new publisher with ID: {PublisherId}", publisher.Id);
        await _unitOfWork.Publishers.AddAsync(publisher);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Publisher created successfully with ID: {PublisherId}", publisher.Id);
        return publisher;
    }

    private async Task<Publisher> UpdatePublisherRecord(Publisher publisher)
    {
        _logger.LogInformation("Updating existing publisher with ID: {PublisherId}", publisher.Id);
        await _unitOfWork.Publishers.UpdateAsync(publisher);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Publisher updated successfully with ID: {PublisherId}", publisher.Id);
        return publisher;
    }

    private async Task DeletePublisherEntity(Guid id)
    {
        await _unitOfWork.Publishers.DeleteAsync(id);
        await _unitOfWork.CompleteAsync();
    }

    private async Task<Publisher> FindPublisherByName(string publisherName)
    {
        var publishers = await _unitOfWork.Publishers.GetAllAsync();
        var publisher = publishers
            .FirstOrDefault(p => p.CompanyName.Equals(publisherName, StringComparison.OrdinalIgnoreCase));

        if (publisher == null)
        {
            _logger.LogWarning("Publisher with name {PublisherName} not found", publisherName);
            throw new KeyNotFoundException($"Publisher with name '{publisherName}' not found");
        }

        return publisher;
    }

    private async Task<List<Game>> FindGamesForPublisher(Guid publisherId)
    {
        var games = await _unitOfWork.Publishers.GetGamesByPublisherIdAsync(publisherId);
        var gameList = games.ToList();
        return gameList;
    }

    private async Task<Game> FindGameByKey(string gameKey)
    {
        var game = await _unitOfWork.Games.GetKeyAsync(gameKey);

        if (game == null)
        {
            _logger.LogWarning("Game with key: {GameKey} not found", gameKey);
            throw new KeyNotFoundException($"Game with key '{gameKey}' not found");
        }

        return game;
    }

    private void ValidateGameHasPublisher(Game game)
    {
        if (game.PublisherId == null)
        {
            _logger.LogWarning("Game with key: {GameKey} has no publisher assigned", game.Key);
            throw new KeyNotFoundException($"Game with key '{game.Key}' has no publisher assigned");
        }
    }

    private async Task<Publisher> FindPublisherById(Guid publisherId)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(publisherId);

        if (publisher == null)
        {
            _logger.LogWarning("Publisher with ID: {PublisherId} not found", publisherId);
            throw new KeyNotFoundException($"Publisher with ID '{publisherId}' not found");
        }

        return publisher;
    }

    private void LogPublisherRetrievalResult(Publisher? publisher, Guid id)
    {
        if (publisher != null)
        {
            _logger.LogInformation("Publisher found with ID: {PublisherId}", id);
        }
        else
        {
            _logger.LogWarning("Publisher not found with ID: {PublisherId}", id);
        }
    }

    private void LogPublisherNameRetrievalResult(Publisher? publisher, string companyName)
    {
        if (publisher != null)
        {
            _logger.LogInformation("Publisher found with company name: {CompanyName}", companyName);
        }
        else
        {
            _logger.LogWarning("Publisher not found with company name: {CompanyName}", companyName);
        }
    }

    private static Publisher EnsurePublisherHasId(Publisher publisher)
    {
        if (publisher.Id == Guid.Empty)
        {
            publisher.Id = Guid.NewGuid();
        }

        return publisher;
    }

    private async Task<Publisher> ValidatePublisherExists(Guid id)
    {
        var publisher = await _unitOfWork.Publishers.GetByIdAsync(id);
        if (publisher == null)
        {
            _logger.LogWarning("Publisher with ID: {PublisherId} not found", id);
            throw new KeyNotFoundException($"Publisher with ID {id} not found");
        }

        return publisher;
    }

    private void ValidatePublisherEntity(Publisher? publisher)
    {
        if (publisher == null)
        {
            _logger.LogWarning("Publisher entity is null");
            throw new ArgumentNullException(nameof(publisher), "Publisher cannot be null");
        }

        if (string.IsNullOrWhiteSpace(publisher.CompanyName))
        {
            _logger.LogWarning("Publisher company name is empty");
            throw new ValidationException("Publisher company name cannot be empty");
        }
    }

    private void ValidatePublisherId(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Provided publisher ID is empty");
            throw new ArgumentException("Publisher ID cannot be empty", nameof(id));
        }
    }

    private void ValidateCompanyName(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            _logger.LogWarning("Provided company name is null or empty");
            throw new ArgumentException("Company name cannot be null or empty", nameof(companyName));
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

    private static PublisherDto MapToPublisherDto(Publisher publisher)
    {
        return new PublisherDto
        {
            Id = publisher.Id,
            CompanyName = publisher.CompanyName,
            HomePage = publisher.HomePage,
            Description = publisher.Description,
        };
    }
}