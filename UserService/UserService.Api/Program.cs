using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using UserService.Application;
using UserService.Application.Features.ShopperAssistants.Validators;
using UserService.Application.Interfaces.Data;
using UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining<CreateShopperAssistantRequestValidator>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<UserServiceDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IUserServiceDbContext>(provider =>
    provider.GetRequiredService<UserServiceDbContext>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();