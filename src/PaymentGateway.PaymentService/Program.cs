using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Infrastructure.Services;
using PaymentGateway.Infrastructure.Middleware;
using PaymentGateway.Domain.Interfaces;
using PaymentGateway.Domain.Validators;
using StackExchange.Redis;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<InitiatePaymentRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
if (!builder.Services.Any(x => x.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Configure Redis for Idempotency
if (!builder.Services.Any(x => x.ServiceType == typeof(IConnectionMultiplexer)))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
        ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
}
builder.Services.AddScoped<IIdempotencyService, RedisIdempotencyService>();

// Configure MassTransit for RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
