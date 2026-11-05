namespace ProductService.Application.DTOs.Request;

public class CreateCategoryRequestRequest
{
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
}