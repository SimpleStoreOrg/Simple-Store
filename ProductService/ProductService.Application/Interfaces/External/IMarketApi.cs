using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs.External;
using Refit;

namespace ProductService.Application.Interfaces.External;

public interface IMarketApi
{
    [Get("/api/Market/{id}")]
    Task<MarketResponse?> GetMarketById(long id, [Header("Authorization")] string? authorization);
}