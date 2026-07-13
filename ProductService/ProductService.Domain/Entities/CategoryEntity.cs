namespace ProductService.Domain.Entities;

public class CategoryEntity : BaseEntity<long>
{
    public string Name { get; set; }
    public long? ParentCategoryId { get; set; }
    public List<CategoryEntity> SubCategories { get; set; }
    public List<ProductEntity> Products { get; set; }
}