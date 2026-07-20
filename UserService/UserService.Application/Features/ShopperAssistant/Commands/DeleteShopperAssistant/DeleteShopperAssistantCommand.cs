using MediatR;

namespace UserService.Application.Features.ShopperAssistant.Commands.DeleteShopperAssistant;

public record DeleteShopperAssistantCommand(long ShopperAssistantId) : IRequest, IRequest<bool>;