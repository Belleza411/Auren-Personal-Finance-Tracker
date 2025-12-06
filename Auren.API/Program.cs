using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Auren.Application;
using Auren.API.Extensions;
using Microsoft.Extensions.Configuration;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Auren.Application.Interfaces.Repositories;
using Auren.API.Middleware;
using Auren.Infrastructure.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.AddPresentationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

using (var scope = app.Services.CreateScope())
{
    var authDbContext = scope.ServiceProvider.GetRequiredService<AurenAuthDbContext>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AurenDbContext>();

    authDbContext.Database.EnsureCreated();
    dbContext.Database.EnsureCreated();
}

app.MapControllers();

app.Run();
