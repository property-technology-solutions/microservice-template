using BuildingBlocks.Application;
using BuildingBlocks.Domain.Repositories;
using BuildingBlocks.Infrastructure.FeatureFlags;
using HakuService.Application.Features.Hakus.Commands.CreateHaku;
using HakuService.Application.Specifications;
using HakuService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HakuService.Application.Features.Hakus.Queries.GetHakuList;

/// <summary>
/// Handler for GetHakuListQuery
/// Demonstrates: Repository Pattern, Specification Pattern, Feature Flags
/// </summary>
public class GetHakuListQueryHandler : IRequestHandler<GetHakuListQuery, Result<PagedResult<HakuResponse>>>
{
    private readonly IReadRepository<Haku> _hakuRepository;
    private readonly IFeatureFlagService _featureFlags;
    private readonly ILogger<GetHakuListQueryHandler> _logger;

    public GetHakuListQueryHandler(
        IReadRepository<Haku> hakuRepository,
        IFeatureFlagService featureFlags,
        ILogger<GetHakuListQueryHandler> logger)
    {
        _hakuRepository = hakuRepository;
        _featureFlags = featureFlags;
        _logger = logger;
    }

    public async Task<Result<PagedResult<HakuResponse>>> Handle(
        GetHakuListQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting Haku list - Page: {Page}, Size: {Size}, SSId: {SSId}",
            request.PageNumber, request.PageSize, request.SSId);

        // Example: Feature Flag usage in handler
        var includeFeatured = _featureFlags.IsEnabled("ShowFeaturedFirst");

        // Use Specification Pattern for reusable query logic
        IReadOnlyList<Haku> hakusList;
        int totalCount;

        if (request.SSId.HasValue)
        {
            // Filter by Shopping Center using Specification
            var spec = new HakusByShoppingCenterSpecification(request.SSId.Value);
            totalCount = await _hakuRepository.CountAsync(spec, cancellationToken);
            hakusList = await _hakuRepository.ListAsync(spec, cancellationToken);
        }
        else
        {
            // Use paginated specification
            var countSpec = new ActiveHakusSpecification();
            var listSpec = new ActiveHakusSpecification(request.PageNumber, request.PageSize);
            
            totalCount = await _hakuRepository.CountAsync(countSpec, cancellationToken);
            hakusList = await _hakuRepository.ListAsync(listSpec, cancellationToken);
        }

        // Example: Conditional logic based on feature flag
        IEnumerable<Haku> orderedList = includeFeatured
            ? hakusList.OrderByDescending(h => h.IsFeatured).ThenBy(h => h.Name)
            : hakusList;

        var hakus = orderedList.Select(h => new HakuResponse(
            h.Id,
            h.Name,
            h.SSId,
            h.IsFeatured,
            h.Created
        )).ToList();

        var pagedResult = new PagedResult<HakuResponse>(
            hakus,
            request.PageNumber,
            request.PageSize,
            totalCount
        );

        return Result<PagedResult<HakuResponse>>.Success(pagedResult);
    }
}
