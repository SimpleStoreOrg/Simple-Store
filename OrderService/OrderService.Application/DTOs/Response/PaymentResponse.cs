namespace OrderService.Application.DTOs.Response;

public class PaymentResponse
{
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Change { get; set; }
}