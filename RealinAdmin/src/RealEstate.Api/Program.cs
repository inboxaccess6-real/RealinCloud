using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Application;
using RealEstate.Infrastructure;
using RealEstate.Api.Endpoints;
using RealEstate.Api.Endpoints.Admin;
using RealEstate.Api.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DI - Application + Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RealinApi",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RealinApp",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]
                    ?? throw new InvalidOperationException("JWT SecretKey is required"))),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS - allow Admin WASM origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// app.UseHttpsRedirection();

// Serve uploaded media files
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads", "media");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads/media"
});

app.UseCors("AppCors");
app.UseAuthentication();
app.UseAuthorization();

// Map all endpoints
app.MapAuthEndpoints();
app.MapPropertyEndpoints();
app.MapAgentEndpoints();
app.MapBuilderEndpoints();
app.MapProjectEndpoints();
app.MapUserEndpoints();
app.MapRoleEndpoints();
app.MapModuleEndpoints();
app.MapApprovalEndpoints();
app.MapAuditLogEndpoints();
app.MapDashboardEndpoints();
app.MapMediaEndpoints();

app.Run();
