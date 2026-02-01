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
using CoreBackend.Domain.Entities;



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

// Authentication
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

	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			if (context.Exception is SecurityTokenExpiredException)
			{
				context.Response.Headers.Append("X-Token-Expired", "true");
			}
			return Task.CompletedTask;
		},
		OnTokenValidated = context =>
		{
			// Token doðrulandýðýnda session kontrolü yapýlabilir
			return Task.CompletedTask;
		}
	};
});

builder.Services.AddAuthorization();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
	options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
	options.AssumeDefaultVersionWhenUnspecified = true;
	options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
	options.GroupNameFormat = "'v'VVV";
	options.SubstituteApiVersionInUrl = true;
});

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

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
	options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});



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