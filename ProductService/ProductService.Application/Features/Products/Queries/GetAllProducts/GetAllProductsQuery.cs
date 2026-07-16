using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Products.Queries.GetAllProducts;

public record GetAllProductsQuery(int PageNumber, int PageSize) : IRequest<PagedResponse<ProductResponse>>;