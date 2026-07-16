using MediatR;

namespace ProductService.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(long CategoryId) : IRequest,IRequest<bool>;