using System.Text;
using System.Text.Json;
using AIAgentMiddleware.Models;

namespace AIAgentMiddleware.Services;

public interface IGPTService
{
    Task<string> GenerateCodeAsync(string prompt, string guidance);
    Task<string> RefactorCodeAsync(string code, string instructions);
    Task<string> GenerateTestsAsync(string code, string testFramework);
}

public class GPTService : IGPTService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GPTService> _logger;

    public GPTService(HttpClient httpClient, IConfiguration configuration, ILogger<GPTService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Configuration des headers pour l'API OpenAI
        var apiKey = _configuration["ApiKeys:OpenAI"] ?? throw new InvalidOperationException("OpenAI API key not found");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> GenerateCodeAsync(string prompt, string guidance)
    {
        _logger.LogInformation("Génération de code avec GPT");

        var systemPrompt = @"Tu es un expert développeur C# spécialisé en Blazor WebAssembly et MudBlazor. 
Tu génères du code propre, bien structuré et directement utilisable.
Respecte toujours les conventions C# et les bonnes pratiques de développement.";

        var userPrompt = $@"{guidance}

DEMANDE SPÉCIFIQUE:
{prompt}

Génère du code C# complet et fonctionnel. Format ta réponse ainsi:
1. Explication brève de ce que fait le code
2. Le code complet entre ```csharp et ```
3. Instructions d'intégration si nécessaire";

        var request = new OpenAIRequest
        {
            Model = "gpt-4",
            Messages = new List<OpenAIMessage>
            {
                new OpenAIMessage { Role = "system", Content = systemPrompt },
                new OpenAIMessage { Role = "user", Content = userPrompt }
            },
            MaxTokens = 4000,
            Temperature = 0.1
        };

        return await SendOpenAIRequestAsync(request);
    }

    public async Task<string> RefactorCodeAsync(string code, string instructions)
    {
        _logger.LogInformation("Refactoring de code avec GPT");

        var systemPrompt = @"Tu es un expert en refactoring C#. 
Tu améliores la qualité du code tout en préservant sa fonctionnalité.
Tu appliques les principes SOLID et les design patterns appropriés.";

        var userPrompt = $@"Refactorise ce code C# selon les instructions suivantes:

INSTRUCTIONS DE REFACTORING:
{instructions}

CODE À REFACTORISER:
```csharp
{code}
```

Fournis:
1. Explication des améliorations apportées
2. Le code refactorisé complet entre ```csharp et ```
3. Justification des changements effectués";

        var request = new OpenAIRequest
        {
            Model = "gpt-4",
            Messages = new List<OpenAIMessage>
            {
                new OpenAIMessage { Role = "system", Content = systemPrompt },
                new OpenAIMessage { Role = "user", Content = userPrompt }
            },
            MaxTokens = 4000,
            Temperature = 0.1
        };

        return await SendOpenAIRequestAsync(request);
    }

    public async Task<string> GenerateTestsAsync(string code, string testFramework)
    {
        _logger.LogInformation("Génération de tests avec GPT pour framework: {Framework}", testFramework);

        var systemPrompt = $@"Tu es un expert en tests unitaires C# avec {testFramework}.
Tu génères des tests complets, bien structuré et qui couvrent tous les cas d'usage importants.
Tu utilises les meilleures pratiques de testing et les mocking frameworks appropriés.";

        var userPrompt = $@"Génère des tests unitaires complets pour ce code C#:

```csharp
{code}
```

REQUIREMENTS:
- Framework de test: {testFramework}
- Framework de mocking: Moq (si nécessaire)
- Couvre les cas nominaux et les cas d'erreur
- Utilise des noms de test descriptifs
- Ajoute les using statements nécessaires

Format de réponse:
1. Brève explication de la stratégie de test
2. Code complet de la classe de test entre ```csharp et ```
3. Instructions pour l'intégration dans le projet";

        var request = new OpenAIRequest
        {
            Model = "gpt-4",
            Messages = new List<OpenAIMessage>
            {
                new OpenAIMessage { Role = "system", Content = systemPrompt },
                new OpenAIMessage { Role = "user", Content = userPrompt }
            },
            MaxTokens = 4000,
            Temperature = 0.1
        };

        return await SendOpenAIRequestAsync(request);
    }

    private async Task<string> SendOpenAIRequestAsync(OpenAIRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var gptResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return gptResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "Aucune réponse de GPT";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à OpenAI API");
            throw new Exception($"Erreur GPT API: {ex.Message}");
        }
    }
}