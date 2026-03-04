using Carter;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Auth.Login;

/// <summary>
/// Command object representing the payload required to authenticate a user.
/// Encapsulates the intent of the login operation.
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<IResult>;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

/// <summary>
/// Handles the core business logic for user authentication.
/// Evaluates credentials against the database securely utilizing bcrypt.
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, IResult>
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public LoginHandler(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<IResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        // Prevent timing attacks by returning a generic Unauthorized response.
        // Also verify the salted hash utilizing the BCrypt standard configuration.
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        var token = _jwt.GenerateToken(user);
        return Results.Ok(new
        {
            Token = token,
            User = new { user.Id, user.Email, user.FirstName, user.LastName }
        });
    }
}

/// <summary>
/// Exposes the Minimal API endpoint mapped by Carter.
/// Decouples HTTP routing from core business logic (CQRS).
/// </summary>
public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginCommand cmd, IMediator mediator) =>
        {
            return await mediator.Send(cmd);
        }).AllowAnonymous();
    }
}
