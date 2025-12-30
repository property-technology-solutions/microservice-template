using BuildingBlocks.API.Controllers;
using HakuService.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HakuService.API.Controllers;

/// <summary>
/// Admin controller for managing Haku translations.
/// Provides multi-language content management.
/// </summary>
[Route("api/admin/hakus/{hakuId:guid}/translations")]
[Authorize(Roles = "Admin")]
public class TranslationsController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TranslationsController> _logger;

    public TranslationsController(IApplicationDbContext context, ILogger<TranslationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all translations for a Haku.
    /// </summary>
    /// <param name="hakuId">Haku ID</param>
    /// <returns>List of translations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TranslationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTranslations(Guid hakuId)
    {
        var hakuExists = await _context.Hakus.AnyAsync(h => h.Id == hakuId);
        if (!hakuExists)
            return ApiNotFound($"Haku with ID {hakuId} was not found.");

        var translations = await _context.HakuTranslations
            .Where(t => t.HakuId == hakuId && t.Status == 1)
            .Select(t => new TranslationResponse(
                t.Id,
                t.LanguageCode,
                t.Name
            ))
            .ToListAsync();

        return ApiOk(translations, "Translations retrieved successfully.");
    }

    /// <summary>
    /// Add or update translation for a Haku.
    /// </summary>
    /// <param name="hakuId">Haku ID</param>
    /// <param name="languageCode">Language code (e.g., en, tr, de)</param>
    /// <param name="request">Translation data</param>
    /// <returns>Success message</returns>
    [HttpPut("{languageCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetTranslation(
        Guid hakuId, 
        string languageCode, 
        [FromBody] SetTranslationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiBadRequest("Translation name is required.");

        var haku = await _context.Hakus
            .Include(h => h.Translations)
            .FirstOrDefaultAsync(h => h.Id == hakuId);

        if (haku == null)
            return ApiNotFound($"Haku with ID {hakuId} was not found.");

        haku.SetTranslation(languageCode, request.Name);
        
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Translation updated for Haku {HakuId}, Language: {Language}", 
            hakuId, languageCode);

        return ApiOk(message: "Translation updated successfully.");
    }

    /// <summary>
    /// Delete translation for a Haku.
    /// </summary>
    /// <param name="hakuId">Haku ID</param>
    /// <param name="languageCode">Language code to delete</param>
    /// <returns>Success message</returns>
    [HttpDelete("{languageCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTranslation(Guid hakuId, string languageCode)
    {
        var haku = await _context.Hakus
            .Include(h => h.Translations)
            .FirstOrDefaultAsync(h => h.Id == hakuId);

        if (haku == null)
            return ApiNotFound($"Haku with ID {hakuId} was not found.");

        haku.RemoveTranslation(languageCode);
        
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Translation deleted for Haku {HakuId}, Language: {Language}", 
            hakuId, languageCode);

        return ApiOk(message: "Translation deleted successfully.");
    }
}

/// <summary>
/// Request model for setting a translation.
/// </summary>
/// <param name="Name">Translated name</param>
public record SetTranslationRequest(string Name);

/// <summary>
/// Response model for translation data.
/// </summary>
/// <param name="Id">Translation ID</param>
/// <param name="LanguageCode">Language code</param>
/// <param name="Name">Translated name</param>
public record TranslationResponse(Guid Id, string LanguageCode, string Name);
