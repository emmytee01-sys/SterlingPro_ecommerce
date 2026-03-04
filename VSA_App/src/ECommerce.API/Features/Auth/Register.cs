using Carter;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Auth.Register;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<IResult>;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}

public class RegisterHandler : IRequestHandler<RegisterCommand, IResult>
{
    private readonly AppDbContext _db;

    public RegisterHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return Results.BadRequest(new { error = "Email is already taken." });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { message = "Registration successful" });
    }
}

public class RegisterEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterCommand cmd, IMediator mediator) =>
        {
            return await mediator.Send(cmd);
        }).AllowAnonymous();
    }
}
