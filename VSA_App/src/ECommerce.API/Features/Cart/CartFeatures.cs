using Carter;
using ECommerce.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ECommerce.API.Features.Cart;

/// <summary>
/// Defines the HTTP endpoints for the Cart module.
/// By keeping these within the Vertical Slice, all dependencies and routing 
/// pertaining to 'Cart' reside completely independently.
/// </summary>
public class CartEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cart").RequireAuthorization();

        group.MapGet("/", async (HttpContext ctx, IMediator mediator) => 
            await mediator.Send(new GetCartQuery(GetUserId(ctx))));

        group.MapPost("/items", async (HttpContext ctx, AddToCartCommand cmd, IMediator mediator) => 
            await mediator.Send(cmd with { UserId = GetUserId(ctx) }));

        group.MapPut("/items/{productId}", async (HttpContext ctx, Guid productId, UpdateCartItemCommand cmd, IMediator mediator) => 
            await mediator.Send(cmd with { UserId = GetUserId(ctx), ProductId = productId }));

        group.MapDelete("/items/{productId}", async (HttpContext ctx, Guid productId, IMediator mediator) => 
            await mediator.Send(new RemoveFromCartCommand(GetUserId(ctx), productId)));

        group.MapDelete("/", async (HttpContext ctx, IMediator mediator) => 
            await mediator.Send(new ClearCartCommand(GetUserId(ctx))));
    }

    private static Guid GetUserId(HttpContext ctx) 
    {
        var idClaim = ctx.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)
                   ?? ctx.User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        return Guid.Parse(idClaim!.Value);
    }
}

// Features
public record GetCartQuery(Guid UserId) : IRequest<IResult>;
public class GetCartHandler : IRequestHandler<GetCartQuery, IResult>
{
    private readonly AppDbContext _db;
    public GetCartHandler(AppDbContext db) => _db = db;
    public async Task<IResult> Handle(GetCartQuery req, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);
        if (cart == null) return Results.Ok(new { Items = Array.Empty<object>(), TotalItems = 0, TotalPrice = 0 });

        return Results.Ok(new {
            Items = cart.Items.Select(i => new {
                i.Product.Id, i.Product.Name, i.Product.ImageUrl, i.Product.StockQuantity,
                i.Quantity, Price = i.UnitPrice
            }),
            TotalItems = cart.Items.Sum(i => i.Quantity),
            TotalPrice = cart.Items.Sum(i => i.Quantity * i.UnitPrice)
        });
    }
}

public record AddToCartCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest<IResult>;
public class AddToCartHandler : IRequestHandler<AddToCartCommand, IResult>
{
    private readonly AppDbContext _db;
    public AddToCartHandler(AppDbContext db) => _db = db;
    public async Task<IResult> Handle(AddToCartCommand req, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);
        if (cart == null)
        {
            cart = new Domain.Entities.Cart { Id = Guid.NewGuid(), UserId = req.UserId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _db.Carts.Add(cart);
        }

        var product = await _db.Products.FindAsync(new object[] { req.ProductId }, ct);
        if (product == null) return Results.NotFound("Product not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == req.ProductId);
        if (item != null) item.Quantity += req.Quantity;
        else cart.Items.Add(new Domain.Entities.CartItem { ProductId = req.ProductId, Quantity = req.Quantity, UnitPrice = product.Price });

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Results.Ok();
    }
}

public record UpdateCartItemCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest<IResult>;
public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, IResult>
{
    private readonly AppDbContext _db;
    public UpdateCartItemHandler(AppDbContext db) => _db = db;
    public async Task<IResult> Handle(UpdateCartItemCommand req, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);
        if (cart == null) return Results.NotFound();

        var item = cart.Items.FirstOrDefault(i => i.ProductId == req.ProductId);
        if (item == null) return Results.NotFound();

        if (req.Quantity <= 0) cart.Items.Remove(item);
        else item.Quantity = req.Quantity;

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Results.Ok();
    }
}

public record RemoveFromCartCommand(Guid UserId, Guid ProductId) : IRequest<IResult>;
public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand, IResult>
{
    private readonly AppDbContext _db;
    public RemoveFromCartHandler(AppDbContext db) => _db = db;
    public async Task<IResult> Handle(RemoveFromCartCommand req, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);
        if (cart == null) return Results.NotFound();

        var item = cart.Items.FirstOrDefault(i => i.ProductId == req.ProductId);
        if (item != null) cart.Items.Remove(item);

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Results.Ok();
    }
}

public record ClearCartCommand(Guid UserId) : IRequest<IResult>;
public class ClearCartHandler : IRequestHandler<ClearCartCommand, IResult>
{
    private readonly AppDbContext _db;
    public ClearCartHandler(AppDbContext db) => _db = db;
    public async Task<IResult> Handle(ClearCartCommand req, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);
        if (cart != null)
        {
            cart.Items.Clear();
            await _db.SaveChangesAsync(ct);
        }
        return Results.Ok();
    }
}
