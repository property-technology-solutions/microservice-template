using Asp.Versioning;
using BuildingBlocks.API.Controllers;
using BuildingBlocks.API.Filters;
using BuildingBlocks.Application;
using HakuService.Application.Features.Hakus.Commands.CreateHaku;
using HakuService.Application.Features.Hakus.Queries.GetHaku;
using HakuService.Application.Features.Hakus.Queries.GetHakuList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HakuService.API.Controllers;

/// <summary>
/// Version 1 of Hakus API.
/// Provides CRUD operations for Haku entities.
/// Demonstrates: API Versioning, Feature Flags, Result Pattern
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class HakusController : BaseApiController
{
    private readonly ISender _sender;
    private readonly ILogger<HakusController> _logger;

    public HakusController(ISender sender, ILogger<HakusController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Haku.
    /// </summary>
    /// <param name="command">Haku creation data</param>
    /// <returns>Created Haku</returns>
    [HttpPost]
    [ProducesResponseType(typeof(HakuResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateHakuCommand command)
    {
        var result = await _sender.Send(command);

        if (result.IsFailure)
            return ApiBadRequest(result.Errors);

        return ApiCreated(result.Value!, nameof(GetById), new { version = "1.0", id = result.Value!.Id }, result.Message);
    }

    /// <summary>
    /// Get Haku by ID.
    /// </summary>
    /// <param name="id">Haku ID</param>
    /// <returns>Haku details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HakuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _sender.Send(new GetHakuQuery(id));

        if (result.IsFailure)
            return ApiNotFound($"Haku with ID {id} was not found.");

        return ApiOk(result.Value!, "Haku retrieved successfully.");
    }

    /// <summary>
    /// Get paginated list of Hakus.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="ssId">Optional Shopping Center ID filter</param>
    /// <returns>Paginated list of Hakus</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<HakuResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] int? ssId = null)
    {
        var result = await _sender.Send(new GetHakuListQuery(pageNumber, pageSize, ssId));

        if (result.IsFailure)
            return ApiBadRequest(result.Errors);

        return ApiOk(result.Value!, "Hakus retrieved successfully.");
    }

    /// <summary>
    /// Get featured Hakus (Beta Feature).
    /// This endpoint is controlled by Feature Flag.
    /// Returns 404 if feature is disabled.
    /// </summary>
    /// <returns>List of featured Hakus</returns>
    [HttpGet("featured")]
    [FeatureFlag("BetaFeatures")]
    [ProducesResponseType(typeof(PagedResult<HakuResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeatured()
    {
        // This endpoint only works when "BetaFeatures" is enabled in appsettings.json
        var result = await _sender.Send(new GetHakuListQuery(1, 10, null));

        if (result.IsFailure)
            return ApiBadRequest(result.Errors);

        var featured = result.Value!.Items.Where(h => h.IsFeatured).ToList();
        return ApiOk(featured, "Featured Hakus retrieved successfully.");
    }
}
