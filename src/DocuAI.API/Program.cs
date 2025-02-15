using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DocuAI.Infrastructure.AWS;
using DocuAI.Infrastructure.Aspose;
using DocuAI.Core.Features.Documents.Application.Summaries;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register your services for DI.
// Using AddScoped for the command handler is useful if it depends on other scoped services (like DbContext).
builder.Services.AddScoped<GenerateSummaryCommandHandler>();

// Register other services.
builder.Services.AddSingleton<AwsBedrockClient>();
builder.Services.AddSingleton<AsposeComparisonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
