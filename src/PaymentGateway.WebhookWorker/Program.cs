using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.WebhookWorker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClient
builder.Services.AddHttpClient();

// Configure MassTransit for RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentStatusChangedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("payment-status-changed", e =>
        {
            e.ConfigureConsumer<PaymentStatusChangedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
