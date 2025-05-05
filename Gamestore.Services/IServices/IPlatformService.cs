using Gamestore.Entities;
using Gamestore.Services.Dto;

namespace Gamestore.Services.IServices;

public interface IPlatformService
{
    Task<PlatformDto> UpdatePlatform(PlatformRequestDto platformRequest);

    Task<PlatformDto> CreatePlatform(PlatformRequestDto platformRequest);

    Task<Platform> DeletePlatformById(Guid id);

    Task<IEnumerable<Platform>> GetAllPlatformsAsync();

    Task<Platform> GetPlatformById(Guid id);

    Task<IEnumerable<PlatformDto>> GetPlatformsByGameKeyAsync(string gameKey);
}
