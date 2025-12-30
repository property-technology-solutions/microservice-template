using BuildingBlocks.Application;
using BuildingBlocks.Application.Extensions;
using BuildingBlocks.Infrastructure.Cache;
using BuildingBlocks.Infrastructure.Localization;
using HakuService.Application.Common.Interfaces;
using HakuService.Application.Features.Hakus.Commands.CreateHaku;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HakuService.Application.Features.Hakus.Queries.GetHaku;

/// <summary>
/// Handler for GetHakuQuery
/// </summary>
public class GetHakuQueryHandler : IRequestHandler<GetHakuQuery, Result<HakuResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILanguageService _languageService;
    private readonly ILogger<GetHakuQueryHandler> _logger;

    public GetHakuQueryHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILanguageService languageService,
        ILogger<GetHakuQueryHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _languageService = languageService;
        _logger = logger;
    }

    public async Task<Result<HakuResponse>> Handle(
        GetHakuQuery request,
        CancellationToken cancellationToken)
    {
        var language = _languageService.GetCurrentLanguage();
        _logger.LogInformation("Getting Haku {HakuId} in language: {Language}", request.Id, language);

        var cacheKey = $"haku:{request.Id}:{language}";
        var cached = await _cacheService.GetAsync<HakuResponse>(cacheKey, cancellationToken);

        if (cached != null)
        {
            _logger.LogInformation("Haku {HakuId} retrieved from cache", request.Id);
            return Result<HakuResponse>.Success(cached);
        }

        var haku = await _context.Hakus
            .IncludeTranslations()
            .Where(h => h.Id == request.Id && h.Status == 1)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (haku == null)
        {
            return Result<HakuResponse>.Fail($"Haku with ID {request.Id} not found");
        }

        var translation = haku.Translations.FirstOrDefault(t => t.LanguageCode == language);
        var isTranslated = translation != null;

        var response = new HakuResponse(
            haku.Id,
            translation?.Name ?? haku.Name,
            haku.SSId,
            haku.IsFeatured,
            haku.Created,
            language,
            isTranslated
        );

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<HakuResponse>.Success(response);
    }
}
