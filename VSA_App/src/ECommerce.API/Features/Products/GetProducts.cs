using Carter;
using ECommerce.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ECommerce.API.Features.Products.GetProducts;

public record GetProductsQuery(int Page = 1, int PageSize = 12, int? CategoryId = null, string? Search = null, string? SortBy = "price", string? SortOrder = "asc") : IRequest<IResult>;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, IResult>
{
    private readonly AppDbContext _db;

    public GetProductsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.Description.Contains(request.Search));

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => request.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price) // Default by price
        };

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.StockQuantity,
                Category = new { p.Category.Id, p.Category.Name, p.Category.Slug }
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            Data = products,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalItems,
            TotalPages = totalPages
        });
    }
}

public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async ([AsParameters] GetProductsQuery query, IMediator mediator) =>
        {
            return await mediator.Send(query);
        }).AllowAnonymous();
    }
}
