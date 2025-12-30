using BuildingBlocks.Domain.Specifications;
using HakuService.Domain.Entities;

namespace HakuService.Application.Specifications;

/// <summary>
/// Specification for active (non-deleted) Hakus
/// Example of reusable query logic
/// </summary>
public class ActiveHakusSpecification : BaseSpecification<Haku>
{
    public ActiveHakusSpecification() : base(h => h.Status == 1)
    {
        AddInclude(h => h.Translations);
        ApplyOrderBy(h => h.Name);
    }

    public ActiveHakusSpecification(int pageNumber, int pageSize) 
        : base(h => h.Status == 1)
    {
        AddInclude(h => h.Translations);
        ApplyOrderBy(h => h.Name);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}

/// <summary>
/// Specification for featured Hakus
/// </summary>
public class FeaturedHakusSpecification : BaseSpecification<Haku>
{
    public FeaturedHakusSpecification() 
        : base(h => h.Status == 1 && h.IsFeatured)
    {
        AddInclude(h => h.Translations);
        ApplyOrderBy(h => h.OrderNo);
    }
}

/// <summary>
/// Specification for Hakus by Shopping Center
/// </summary>
public class HakusByShoppingCenterSpecification : BaseSpecification<Haku>
{
    public HakusByShoppingCenterSpecification(int ssId) 
        : base(h => h.Status == 1 && h.SSId == ssId)
    {
        AddInclude(h => h.Translations);
        ApplyOrderBy(h => h.Name);
    }
}

