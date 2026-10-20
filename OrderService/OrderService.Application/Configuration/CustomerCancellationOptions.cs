namespace OrderService.Application.Configuration;

public class CustomerCancellationOptions
{
    public CancellationCountPeriod CancellationCountPeriod { get; set; } = new();
}

public class CancellationCountPeriod
{
    public int DurationType { get; set; }
    public int Duration { get; set; }
    public int CompletedOrderCount { get; set; }
}