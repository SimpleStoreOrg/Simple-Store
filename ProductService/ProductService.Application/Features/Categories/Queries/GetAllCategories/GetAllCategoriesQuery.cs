using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery(int PageNumber, int PageSize) : IRequest<PagedResponse<CategoryResponse>>;