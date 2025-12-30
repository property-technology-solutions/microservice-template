using BuildingBlocks.Application;
using MediatR;

namespace HakuService.Application.Features.Hakus.Commands.CreateHaku;

/// <summary>
/// Command to create a new Haku
/// </summary>
public record CreateHakuCommand(
    string Name,
    int SSId
) : IRequest<Result<HakuResponse>>;

/// <summary>
/// Response DTO for Haku
/// </summary>
public record HakuResponse(
    Guid Id,
    string Name,
    int? SSId,
    bool IsFeatured,
    DateTime? Created,
    string Language = "tr",
    bool IsTranslated = false
);
