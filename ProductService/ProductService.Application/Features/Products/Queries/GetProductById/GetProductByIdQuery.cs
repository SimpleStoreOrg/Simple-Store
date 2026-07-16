using MediatR;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int ProductId) : IRequest<ProductResponse>;