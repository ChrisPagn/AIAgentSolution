using System.Text;
using System.Text.Json;
using AIAgentMiddleware.Models;
using AIAgentMiddleware.Utils;

namespace AIAgentMiddleware.Services;

public interface IClaudeService
{
    Task<string> AnalyzeProjectContextAsync(string projectContext, string userMessage);
    Task<string> GenerateCodeSuggestionAsync(string prompt, string codeContext);
}

public class ClaudeService : IClaudeService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(HttpClient httpHttpClient, IConfiguration configuration, ILogger<ClaudeService> logger)
    {
        _httpClient = httpHttpClient;
        _configuration = configuration;
        _logger = logger;

        // Configuration des headers pour l'API Anthropic
        var apiKey = _configuration["ApiKeys:Claude"] ?? throw new InvalidOperationException("Claude API key not found");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> AnalyzeProjectContextAsync(string projectContext, string userMessage)
    {
        _logger.LogInformation("Analyse du contexte projet avec Claude");

        var prompt = BuildAnalysisPrompt(projectContext, userMessage);

        var request = new ClaudeRequest
        {
            Model = "claude-3-5-sonnet-20241022",
            Messages = new List<ClaudeMessage>
            {
                new ClaudeMessage
                {
                    Role = "user",
                    Content = prompt
                }
            },
            MaxTokens = 4000,
            Temperature = 0.3
        };

        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = new SnakeCaseNamingPolicy() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/messages", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseJson, new JsonSerializerOptions { PropertyNamingPolicy = new SnakeCaseNamingPolicy() });

            if (claudeResponse == null)
                throw new Exception("Réponse Claude API nulle");
            if (claudeResponse.Content == null || claudeResponse.Content.Count == 0)
                throw new Exception("Aucun contenu retourné par Claude API");
            if (string.IsNullOrWhiteSpace(claudeResponse.Content[0].Text))
                throw new Exception("Texte de réponse vide de Claude API");

            return claudeResponse.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à Claude API");
            throw new Exception($"Erreur Claude API: {ex.Message}");
        }
    }

    public async Task<string> GenerateCodeSuggestionAsync(string prompt, string codeContext)
    {
        _logger.LogInformation("Génération de suggestion de code avec Claude");

        var enhancedPrompt = BuildCodeGenerationPrompt(prompt, codeContext);

        var request = new ClaudeRequest
        {
            Model = "claude-3-5-sonnet-20241022",
            Messages = new List<ClaudeMessage>
            {
                new ClaudeMessage
                {
                    Role = "user",
                    Content = enhancedPrompt
                }
            },
            MaxTokens = 4000,
            Temperature = 0.1 // Plus déterministe pour le code
        };

        try
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = new SnakeCaseNamingPolicy() });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/messages", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseJson, new JsonSerializerOptions { PropertyNamingPolicy = new SnakeCaseNamingPolicy() });

            if (claudeResponse == null)
                throw new Exception("Réponse Claude API nulle");
            if (claudeResponse.Content == null || claudeResponse.Content.Count == 0)
                throw new Exception("Aucun contenu retourné par Claude API");
            if (string.IsNullOrWhiteSpace(claudeResponse.Content[0].Text))
                throw new Exception("Texte de réponse vide de Claude API");

            return claudeResponse.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération de code avec Claude");
            throw new Exception($"Erreur génération Claude: {ex.Message}");
        }
    }

    private string BuildAnalysisPrompt(string projectContext, string userMessage)
    {
        return $@"Tu es un expert développeur C# avec une spécialisation en Blazor WebAssembly, MudBlazor, et architecture 3-tiers.

CONTEXTE DU PROJET:
{projectContext}

DEMANDE DE L'UTILISATEUR:
{userMessage}

INSTRUCTIONS:
1. Analyse le contexte du projet et comprends la demande de l'utilisateur
2. Identifie les fichiers qui pourraient être concernés par cette demande
3. Propose une approche technique claire et structurée
4. Prends en compte les bonnes pratiques C#, Blazor WebAssembly et l'architecture 3-tiers
5. Si la demande concerne la génération de code, fournis des guidelines précises pour GPT

RÉPONSE ATTENDUE:
- Une analyse de la demande
- Les fichiers/composants potentiellement impactés
- L'approche technique recommandée
- Des suggestions d'amélioration si pertinentes

Réponds de manière concise et technique.";
    }

    private string BuildCodeGenerationPrompt(string prompt, string codeContext)
    {
        return $@"Tu es un assistant de développement spécialisé en C#, Blazor WebAssembly et MudBlazor.

CONTEXTE DU CODE:
{codeContext}

DEMANDE:
{prompt}

INSTRUCTIONS:
1. Génère du code C# propre et bien structuré
2. Respecte les conventions de nommage et les bonnes pratiques
3. Utilise MudBlazor pour les composants UI si nécessaire
4. Implemente l'injection de dépendances correctement
5. Ajoute des commentaires pour les parties complexes
6. Assure-toi que le code est compatible avec Blazor WebAssembly

Format de réponse:
- Explique brièvement ce que fait le code
- Fournis le code complet et fonctionnel
- Indique où placer ce code dans le projet

Le code doit être prêt à être utilisé directement.";
    }
}