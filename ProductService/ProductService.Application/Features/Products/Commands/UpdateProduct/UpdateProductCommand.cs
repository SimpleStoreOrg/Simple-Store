using MediatR;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(int ProductId, UpdateProductRequest Request) : IRequest<ProductResponse>;