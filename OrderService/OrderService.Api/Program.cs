using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Application.Interfaces.Data;
using OrderService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<OrderServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IOrderServiceDbContext>(provider => provider.GetRequiredService<OrderServiceDbContext>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();