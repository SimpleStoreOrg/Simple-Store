using MediatR;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(long CategoryId) : IRequest<CategoryResponse>;