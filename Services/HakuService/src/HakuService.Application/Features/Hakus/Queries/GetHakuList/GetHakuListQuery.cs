using BuildingBlocks.Application;
using HakuService.Application.Features.Hakus.Commands.CreateHaku;
using MediatR;

namespace HakuService.Application.Features.Hakus.Queries.GetHakuList;

/// <summary>
/// Query to get paginated list of Hakus
/// </summary>
public record GetHakuListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    int? SSId = null
) : IRequest<Result<PagedResult<HakuResponse>>>;

