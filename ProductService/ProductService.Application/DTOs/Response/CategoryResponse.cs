using ProductService.Domain.Entities;

namespace ProductService.Application.DTOs.Response;

public class CategoryResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}