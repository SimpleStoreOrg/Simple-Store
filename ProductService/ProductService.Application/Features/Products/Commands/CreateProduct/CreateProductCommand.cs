using MediatR;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductResponse>;