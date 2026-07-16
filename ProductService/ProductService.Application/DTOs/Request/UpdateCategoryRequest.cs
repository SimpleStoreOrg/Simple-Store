namespace ProductService.Application.DTOs.Request;

public class UpdateCategoryRequest
{
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
}