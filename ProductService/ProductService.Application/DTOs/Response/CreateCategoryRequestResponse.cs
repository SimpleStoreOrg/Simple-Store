using ProductService.Domain.Enums;

namespace ProductService.Application.DTOs.Response;

public class CreateCategoryRequestResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
    public long MarketId { get; set; }
    public long MarketAdminId { get; set; }
    public CategoryRequestStatus Status { get; set; }
    public long? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}