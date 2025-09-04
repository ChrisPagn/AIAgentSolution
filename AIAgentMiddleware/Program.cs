using AIAgentMiddleware.Services;
using Microsoft.OpenApi.Models;
using AIAgentMiddleware.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Agent Middleware API", Version = "v1" });
});

// Configuration CORS pour permettre les appels depuis VS Extension
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVSExtension", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Services personnalisés
builder.Services.AddScoped<IClaudeService, ClaudeService>();
builder.Services.AddScoped<IGPTService, GPTService>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Configuration HttpClient
builder.Services.AddHttpClient<IClaudeService, ClaudeService>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient<IGPTService, GPTService>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVSExtension");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 AI Agent Middleware démarré sur http://localhost:5210");
Console.WriteLine("📖 Documentation Swagger: http://localhost:5210/swagger");

app.Run();