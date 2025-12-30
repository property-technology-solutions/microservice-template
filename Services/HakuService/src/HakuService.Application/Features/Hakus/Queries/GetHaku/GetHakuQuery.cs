using BuildingBlocks.Application;
using HakuService.Application.Features.Hakus.Commands.CreateHaku;
using MediatR;

namespace HakuService.Application.Features.Hakus.Queries.GetHaku;

/// <summary>
/// Query to get a single Haku by ID
/// </summary>
public record GetHakuQuery(int Id) : IRequest<Result<HakuResponse>>;

