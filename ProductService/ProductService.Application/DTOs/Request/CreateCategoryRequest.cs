namespace ProductService.Application.DTOs.Request;

public class CreateCategoryRequest
{
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
}