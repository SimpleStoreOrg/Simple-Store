using MediatR;
using UserService.Application.DTOs.Request;
using UserService.Application.DTOs.Response;

namespace UserService.Application.Features.ShopperAssistant.Commands.CreateShopperAssistant;

public record CreateShopperAssistantCommand(CreateUserRequest Request) : IRequest<UserResponse>;