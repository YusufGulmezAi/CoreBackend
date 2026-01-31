using System.Reflection;
using CoreBackend.Application.Common.Behaviors;
using CoreBackend.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBackend.Application;

/// <summary>
/// Application katmanı dependency injection kayıtları.
/// </summary>
public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		// MediatR
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(assembly);
		});

		// FluentValidation - tüm validator'ları otomatik kaydet
		services.AddValidatorsFromAssembly(assembly);

		// MediatR Pipeline Behaviors (sıralama önemli!)
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

		// Business Rule Checker
		services.AddScoped<IBusinessRuleChecker, BusinessRuleChecker>();

		return services;
	}
}