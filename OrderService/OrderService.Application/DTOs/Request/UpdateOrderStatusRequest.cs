using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs.Request;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}