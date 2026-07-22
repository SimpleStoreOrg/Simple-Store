using MediatR;

namespace ProductService.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductId) : IRequest, IRequest<bool>;