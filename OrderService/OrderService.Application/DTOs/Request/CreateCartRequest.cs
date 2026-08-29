namespace OrderService.Application.DTOs.Request;

public class CreateCartRequest
{ 
    public List<CartItemRequest> CartItems { get; set; }
}