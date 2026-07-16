using MediatR;
using ProductService.Application.DTOs.Request;
using ProductService.Application.DTOs.Response;

namespace ProductService.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<CategoryResponse>;