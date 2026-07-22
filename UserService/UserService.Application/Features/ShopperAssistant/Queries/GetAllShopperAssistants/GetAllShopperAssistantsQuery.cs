using MediatR;
using UserService.Application.Common;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.ShopperAssistant.Queries.GetAllShopperAssistants;

public record GetAllShopperAssistantsQuery(int PageNumber, int PageSize) : IRequest<PagedResponse<UserResponse>>;