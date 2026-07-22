using MediatR;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.ShopperAssistant.Commands.UpdateShopperAssistant;

public record UpdateShopperAssistantCommand(long ShopperAssistantId, UpdateUserRequest Request) : IRequest<UserResponse>;