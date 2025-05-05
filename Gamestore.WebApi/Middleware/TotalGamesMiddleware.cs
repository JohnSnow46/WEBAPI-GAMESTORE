using Gamestore.Services.IServices;
using Microsoft.Extensions.Caching.Memory;

namespace Gamestore.WebApi.Middleware;

public class TotalGamesMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, IMemoryCache memoryCache)
{
    private readonly RequestDelegate _next = next;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IMemoryCache _memoryCache = memoryCache;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_memoryCache.TryGetValue("TotalGamesCount", out int totalGamesCount))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
            totalGamesCount = await gameService.GetTotalGamesCountAsync();

            _memoryCache.Set("TotalGamesCount", totalGamesCount, TimeSpan.FromMinutes(1));
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["x-total-numbers-of-games"] = totalGamesCount.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}