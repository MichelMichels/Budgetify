using System.Text.Json;
using System.Text.Json.Serialization;
using Budgetify.Core.Data;
using Budgetify.Core.Interfaces;
using Budgetify.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Register In-Memory Data Store (Singleton so state is shared across requests)
builder.Services.AddSingleton<InMemoryDataStore>();

// Register Repositories
builder.Services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();
builder.Services.AddSingleton<IPartyRepository, InMemoryPartyRepository>();
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
builder.Services.AddSingleton<IBudgetRepository, InMemoryBudgetRepository>();

// Register Services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Configure Controllers with snake_case serialization matching API specs
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
