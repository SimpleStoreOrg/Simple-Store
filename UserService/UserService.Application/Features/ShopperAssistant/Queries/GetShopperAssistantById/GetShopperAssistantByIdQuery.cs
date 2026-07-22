using MediatR;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.ShopperAssistant.Queries.GetShopperAssistantById;

public record GetShopperAssistantByIdQuery(long ShopperAssistantId) : IRequest<UserResponse>;