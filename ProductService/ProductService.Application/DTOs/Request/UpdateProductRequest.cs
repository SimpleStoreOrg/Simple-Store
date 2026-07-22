namespace ProductService.Application.DTOs.Request;

public class UpdateProductRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
    public int CategoryId { get; set; }
}