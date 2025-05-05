using Gamestore.Entities;
using Gamestore.Services.Dto.PublishersDto;

namespace Gamestore.Services.IServices;

public interface IPublisherService
{
    Task<IEnumerable<Publisher>> GetAllPublishersAsync();

    Task<Publisher?> GetPublisherByIdAsync(Guid id);

    Task<Publisher?> GetPublisherByCompanyNameAsync(string companyName);

    Task<PublisherCreateDto> AddPublisherAsync(PublisherCreateDto publisher);

    Task<PublisherDto> DeletePublisherAsync(Guid id);

    Task<IEnumerable<Game>> GetGamesByPublisherNameAsync(string publisherName);

    Task<Publisher> GetPublisherByGameKey(string gameKey);

    Task<Publisher> CreateOrUpdatePublisherAsync(Publisher publisher);
}