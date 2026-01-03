using CarAuctions.Application.Common.Interfaces;
using CarAuctions.Infrastructure.Payment;
using CarAuctions.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarAuctions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IPaymentGateway, MockPaymentGateway>();

        return services;
    }
}
