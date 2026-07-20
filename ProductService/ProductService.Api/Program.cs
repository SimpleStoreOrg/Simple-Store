using Microsoft.EntityFrameworkCore;
using ProductService.Application;
using ProductService.Application.Interfaces.Data;
using ProductService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<ProductServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IProductServiceDbContext>(provider =>
    provider.GetRequiredService<ProductServiceDbContext>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();