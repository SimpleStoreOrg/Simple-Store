using ProductService.Domain.Enums;

namespace ProductService.Domain.Entities;

public class CategoryCreationRequestEntity : BaseEntity<long>
{
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
    public long MarketId { get; set; }
    public long MarketAdminId { get; set; }
    public CategoryRequestStatus Status { get; set; } = CategoryRequestStatus.Pending;
    public long? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}