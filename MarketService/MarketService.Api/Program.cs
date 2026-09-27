using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using MarketService_Application;
using MarketService_Application.Features.Validators;
using MarketService_Application.Interfaces.Data;
using MarketService_Application.Interfaces.External;
using MarketService.Api.Middlewares;
using MarketService.Infrastructure;
using MarketService.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Refit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddScoped<GlobalExceptionHandlingMiddleware>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateMarketRequestValidator>();

builder.Services.AddRefitClient<IMarketAdminApi>().ConfigureHttpClient(c =>
{
    c.BaseAddress = new Uri("https://localhost:7003");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your raw JWT token string (Do not type 'Bearer')."
    });
    options.AddSecurityRequirement(document=>new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("AdminPosition", "SuperAdmin");
    });
    
    options.AddPolicy("CustomerOrAdmin", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.IsInRole("Customer") ||
            context.User.IsInRole("ShopperAssistant") ||
            context.User.IsInRole("Admin"));
    });
});
    

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<MarketServiceDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);

    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

builder.Services.AddScoped<AuditInterceptor>();

builder.Services.AddScoped<IMarketServiceDbContext>(provider => provider.GetRequiredService<MarketServiceDbContext>());

var app = builder.Build();

app.UseCors("ReactFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();