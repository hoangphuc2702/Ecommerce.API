using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Application.Features.Product.Commands;

public record UpdateProductStatusCommand(Guid Id, bool IsDeleted) : IRequest<Result<Guid>>;

public class UpdateProductStatusHandler : IRequestHandler<UpdateProductStatusCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProductStatusHandler> _logger;

    public UpdateProductStatusHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProductStatusHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to update status for product ID: {ProductId} to IsDeleted = {Status}", 
            request.Id, request.IsDeleted);

        var product = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Update status failed: Product {ProductId} not found.", request.Id);

            return Result<Guid>.Failure(new Error("Product.NotFound", "Not Found Product"));
        }

        product.IsDeleted = request.IsDeleted;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} status updated successfully to IsDeleted = {Status}.", 
            request.Id, request.IsDeleted);

        return Result<Guid>.SuccessResult(product.Id, "Update product status successfully.");
    }
}