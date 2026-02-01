using CoreBackend.Application.Common.Settings;
using CoreBackend.Api.Extensions;
using CoreBackend.Api.Middlewares;
using CoreBackend.Application;
using CoreBackend.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERILOG CONFIGURATION
// ============================================
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.Enrich.WithMachineName()
	.Enrich.WithEnvironmentName()
	.CreateLogger();

builder.Host.UseSerilog();

// ============================================
// CONFIGURATION
// ============================================
builder.Services.Configure<JwtSettings>(
	builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<SessionSettings>(
	builder.Configuration.GetSection(SessionSettings.SectionName));

// ============================================
// APPLICATION & INFRASTRUCTURE
// ============================================
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ============================================
// API VERSIONING
// ============================================
builder.Services.AddApiVersioningConfiguration();

// ============================================
// AUTHENTICATION
// ============================================
var jwtSettings = builder.Configuration
	.GetSection(JwtSettings.SectionName)
	.Get<JwtSettings>()!;

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings.Issuer,
		ValidAudience = jwtSettings.Audience,
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
		ClockSkew = TimeSpan.Zero
	};
});

builder.Services.AddAuthorization();

// ============================================
// SWAGGER
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================
// ENDPOINTS
// ============================================
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

// ============================================
// CORS
// ============================================
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

// Exception Handler (en baþta olmalý)
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Serilog request logging
app.UseSerilogRequestLogging();

// Swagger (Development ortamýnda)
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreBackend API v1");
		options.RoutePrefix = "swagger";
	});
}

// CORS
app.UseCors("AllowAll");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Tenant Middleware
app.UseMiddleware<TenantMiddleware>();

// Endpoints
app.MapEndpoints();

// ============================================
// RUN
// ============================================
Log.Information("Starting CoreBackend API...");

try
{
	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}