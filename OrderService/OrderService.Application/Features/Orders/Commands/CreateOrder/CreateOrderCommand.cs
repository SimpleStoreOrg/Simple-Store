using MediatR;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(CreateOrderRequest Request) : IRequest<OrderResponse>;