using MediatR;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(long CategoryId, UpdateCategoryRequest Request) : IRequest<CategoryResponse>;