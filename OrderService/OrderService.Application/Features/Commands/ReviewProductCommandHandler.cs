using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.Request;
using OrderService.Application.DTOs.Response;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces.Data;
using OrderService.Domain.Entities;

namespace OrderService.Application.Features.Commands;

public record ReviewProductCommand(ReviewProductRequest Request) : IRequest<ReviewProductResponse>;

public class ReviewProductCommandHandler: IRequestHandler<ReviewProductCommand, ReviewProductResponse>
{
    private readonly IOrderServiceDbContext _dbContext;
    private readonly ILogger<ReviewProductCommandHandler> _logger;
    private readonly IHttpContextAccessor _accessor;

    public ReviewProductCommandHandler(IOrderServiceDbContext dbContext,
        ILogger<ReviewProductCommandHandler> logger,
        IHttpContextAccessor accessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _accessor = accessor;
    }

    public async Task<ReviewProductResponse> Handle(ReviewProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Review products command execution");

        var customerIdStr = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (customerIdStr == null)
        {
            throw new UnauthorizedAccessException();
        }

        long customerId = long.Parse(customerIdStr);

        var order = await _dbContext
            .Orders.Include(oi => oi.OrderItems).FirstOrDefaultAsync(
                o => o.Id == request.Request.OrderId && o.CustomerId == customerId,
                cancellationToken);
        
        if (order == null)
        {
            _logger.LogWarning("Order with ID {OrderId} not found", order.Id);
            throw new OrderNotFoundException(order.Id);
        }

        var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == request.Request.ProductId);

        if (orderItem == null)
        {
            _logger.LogWarning("Wrong review request");
            throw new WrongReviewRequestException();
        }

        var reviewed = await _dbContext
            .Reviews.AnyAsync(
                r => r.OrderId == request.Request.OrderId && r.ProductId == request.Request.ProductId &&
                     r.CustomerId == customerId, cancellationToken);
        
        if (reviewed)
        {
            _logger.LogWarning("You have already reviewed this product");
            throw new AlreadyReviewedException();
        }

        var review = new ReviewEntity
        {
            OrderId = request.Request.OrderId,
            ProductId = request.Request.ProductId,
            CustomerId = customerId,
            Rating = request.Request.Rating,
            Message = request.Request.Message
        };

         _dbContext.Reviews.Add(review);
         await _dbContext.SaveChangesAsync(cancellationToken);

         return new ReviewProductResponse
         {
             Id = review.Id,
             ProductId = review.ProductId,
             CustomerId = review.CustomerId,
             Rating = review.Rating,
             Message = review.Message,
             CreatedAt = review.CreatedAt
         };
    }
}