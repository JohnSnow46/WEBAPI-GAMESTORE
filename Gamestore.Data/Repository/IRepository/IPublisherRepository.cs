using Gamestore.Entities;

namespace Gamestore.Data.Repository.IRepository;
public interface IPublisherRepository : IRepository<Publisher>
{
    Task<Publisher?> GetByCompanyNameAsync(string companyName);

    Task<IEnumerable<Game>> GetGamesByPublisherIdAsync(Guid publisherId);
}
