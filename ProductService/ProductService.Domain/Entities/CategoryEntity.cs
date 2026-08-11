namespace ProductService.Domain.Entities;

public class CategoryEntity : BaseEntity<long>
{
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
    public CategoryEntity? ParentCategory { get; set; }
    public List<CategoryEntity> SubCategories { get; set; } = new();
    public List<ProductEntity> Products { get; set; } = new();
}