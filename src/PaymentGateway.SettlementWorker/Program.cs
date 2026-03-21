using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.SettlementWorker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure MassTransit for RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentAuthorizedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("payment-authorized", e =>
        {
            e.ConfigureConsumer<PaymentAuthorizedConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
