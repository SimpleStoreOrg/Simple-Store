namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    New,
    Accepted, 
    Collecting, 
    ReadyToGo, 
    Completed, 
    CancelledByCustomer, 
    CancelledByShop
}