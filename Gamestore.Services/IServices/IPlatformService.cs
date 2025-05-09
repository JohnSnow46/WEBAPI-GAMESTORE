using Gamestore.Entities;
using Gamestore.Services.Dto;

namespace Gamestore.Services.IServices;

public interface IPlatformService
{
    Task<PlatformUpdateDto> UpdatePlatform(Guid id, PlatformUpdateDto platformRequest);

    Task<PlatformDto> CreatePlatform(PlatformRequestDto platformRequest);

    Task<Platform> DeletePlatformById(Guid id);

    Task<IEnumerable<Platform>> GetAllPlatformsAsync();

    Task<Platform> GetPlatformById(Guid id);

    Task<IEnumerable<Platform>> GetPlatformsByGameKeyAsync(string gameKey);
}
