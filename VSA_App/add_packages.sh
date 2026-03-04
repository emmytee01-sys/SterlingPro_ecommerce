#!/bin/bash
cd /Users/macbook/SterlingPro_ecommerce/VSA_App

DOTNET=/Users/macbook/.dotnet/dotnet

$DOTNET add src/ECommerce.API/ECommerce.API.csproj package MediatR
$DOTNET add src/ECommerce.API/ECommerce.API.csproj package FluentValidation.DependencyInjectionExtensions
$DOTNET add src/ECommerce.API/ECommerce.API.csproj package Carter
$DOTNET add src/ECommerce.API/ECommerce.API.csproj package Mapster
$DOTNET add src/ECommerce.API/ECommerce.API.csproj package BCrypt.Net-Next
$DOTNET add src/ECommerce.API/ECommerce.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer -v 8.0.13
$DOTNET add src/ECommerce.API/ECommerce.API.csproj package Microsoft.EntityFrameworkCore.Design -v 8.0.13

$DOTNET add src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer -v 8.0.13
$DOTNET add src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools -v 8.0.13
