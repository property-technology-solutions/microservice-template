using BuildingBlocks.Application;
using BuildingBlocks.Domain.Repositories;
using HakuService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HakuService.Application.Features.Hakus.Commands.CreateHaku;

/// <summary>
/// Handler for CreateHakuCommand
/// Demonstrates: Repository Pattern, Unit of Work
/// </summary>
public class CreateHakuCommandHandler : IRequestHandler<CreateHakuCommand, Result<HakuResponse>>
{
    private readonly IRepository<Haku> _hakuRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHakuCommandHandler> _logger;

    public CreateHakuCommandHandler(
        IRepository<Haku> hakuRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateHakuCommandHandler> logger)
    {
        _hakuRepository = hakuRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<HakuResponse>> Handle(
        CreateHakuCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Haku: {Name} for SSId: {SSId}", request.Name, request.SSId);

        // Create entity using Factory Method (DDD)
        var haku = Haku.Create(request.Name, request.SSId);

        // Use Repository Pattern
        await _hakuRepository.AddAsync(haku, cancellationToken);
        
        // Use Unit of Work Pattern
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Haku created with ID: {HakuId}", haku.Id);

        var response = new HakuResponse(
            haku.Id,
            haku.Name,
            haku.SSId,
            haku.IsFeatured,
            haku.Created
        );

        return Result<HakuResponse>.Success(response, "Haku created successfully");
    }
}
