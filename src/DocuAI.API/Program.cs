using Amazon;
using Amazon.Runtime;
using DocuAI.Core.Features.Documents.Application.Summaries;
using DocuAI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register infrastructure services (this should register services from both Infra and Core).
builder.Services.AddInfrastructureServices(builder.Configuration);


// Register the command handler.
builder.Services.AddScoped<GenerateSummarySyncCommandHandler>();
builder.Services.AddScoped<ReviewDocumentDraftCommandHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
