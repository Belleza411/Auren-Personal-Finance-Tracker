using Auren.API.Extensions;
using Auren.Infrastructure.Persistence;
using Auren.Infrastructure.Extensions;
using Auren.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices(builder.Configuration);
builder.AddApplicationServices();
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

app.UseCors("Auren");

app.MapControllers();

app.Run();
