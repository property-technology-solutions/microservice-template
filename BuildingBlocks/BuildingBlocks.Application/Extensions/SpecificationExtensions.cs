using BuildingBlocks.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Extensions;

/// <summary>
/// Extension methods for applying specifications to queries
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Apply specification to query
    /// Handles criteria, includes, ordering, and paging
    /// </summary>
    public static IQueryable<T> ApplySpecification<T>(
        this IQueryable<T> query,
        ISpecification<T> spec) where T : class
    {
        // Apply criteria
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // Apply includes
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply include strings
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // Apply paging
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}

