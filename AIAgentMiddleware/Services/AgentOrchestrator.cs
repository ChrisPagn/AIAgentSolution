using AIAgentMiddleware.Models;
using System.Text.RegularExpressions;

namespace AIAgentMiddleware.Services;

public interface IAgentOrchestrator
{
    Task<AgentResponse> ProcessRequestAsync(AgentRequest request);
}

public class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IClaudeService _claudeService;
    private readonly IGPTService _gptService;
    private readonly ILogger<AgentOrchestrator> _logger;

    public AgentOrchestrator(
        IClaudeService claudeService,
        IGPTService gptService,
        ILogger<AgentOrchestrator> logger)
    {
        _claudeService = claudeService;
        _gptService = gptService;
        _logger = logger;
    }

    public async Task<AgentResponse> ProcessRequestAsync(AgentRequest request)
    {
        _logger.LogInformation("Traitement de la requête: {Instruction}", request.Instruction);

        try
        {
            return request.Instruction switch
            {
                "analyze-code" => await AnalyzeCodeAsync(request),
                "refactor" => await RefactorCodeAsync(request),
                "generate-tests" => await GenerateTestsAsync(request),
                "generate-code" => await GenerateCodeAsync(request),
                _ => await ProcessGeneralRequestAsync(request)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de la requête");
            return new AgentResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                ResponseText = "❌ Une erreur s'est produite lors du traitement de votre demande."
            };
        }
    }

    private async Task<AgentResponse> ProcessGeneralRequestAsync(AgentRequest request)
    {
        _logger.LogInformation("Traitement général de la requête");

        // Étape 1: Claude analyse le contexte et la demande
        var claudeAnalysis = await _claudeService.AnalyzeProjectContextAsync(
            request.ProjectContext,
            request.Message);

        // Vérifier si une génération de code est nécessaire
        if (IsCodeGenerationRequired(claudeAnalysis, request.Message))
        {
            // Étape 2: GPT génère le code basé sur l'analyse de Claude
            var codeGeneration = await _gptService.GenerateCodeAsync(
                request.Message,
                claudeAnalysis);

            // Étape 3: Extraire le code généré et créer les modifications de fichiers
            var fileModifications = ExtractCodeFromResponse(codeGeneration, request.FilePath);

            return new AgentResponse
            {
                ResponseText = $"📋 **Analyse Claude:**\n{claudeAnalysis}\n\n🔧 **Code généré:**\n{codeGeneration}",
                ModifiedFiles = fileModifications,
                Success = true
            };
        }

        return new AgentResponse
        {
            ResponseText = claudeAnalysis,
            Success = true
        };
    }

    private async Task<AgentResponse> AnalyzeCodeAsync(AgentRequest request)
    {
        _logger.LogInformation("Analyse de code");

        var analysis = await _claudeService.AnalyzeProjectContextAsync(
            request.ProjectContext,
            $"Analyse ce code et donne des suggestions d'amélioration:\n\n{request.SelectedCode}");

        return new AgentResponse
        {
            ResponseText = $"🔍 **Analyse du code:**\n\n{analysis}",
            Success = true
        };
    }

    private async Task<AgentResponse> RefactorCodeAsync(AgentRequest request)
    {
        _logger.LogInformation("Refactoring de code");

        // Claude analyse d'abord le contexte
        var claudeGuidance = await _claudeService.AnalyzeProjectContextAsync(
            request.ProjectContext,
            $"Analyse ce code pour refactoring:\n\n{request.SelectedCode}\n\nObjectif: {request.Message}");

        // GPT effectue le refactoring
        var refactoredCode = await _gptService.RefactorCodeAsync(
            request.SelectedCode ?? "",
            claudeGuidance);

        var fileModifications = ExtractCodeFromResponse(refactoredCode, request.FilePath);

        return new AgentResponse
        {
            ResponseText = $"🔄 **Refactoring effectué:**\n\n{refactoredCode}",
            ModifiedFiles = fileModifications,
            Success = true
        };
    }

    private async Task<AgentResponse> GenerateTestsAsync(AgentRequest request)
    {
        _logger.LogInformation("Génération de tests");

        var tests = await _gptService.GenerateTestsAsync(
            request.SelectedCode ?? "",
            "xUnit"); // Configurable

        // Générer le nom du fichier de test
        var testFilePath = GenerateTestFilePath(request.FilePath);
        var fileModifications = ExtractCodeFromResponse(tests, testFilePath);

        return new AgentResponse
        {
            ResponseText = $"🧪 **Tests générés:**\n\n{tests}",
            ModifiedFiles = fileModifications,
            Success = true
        };
    }

    private async Task<AgentResponse> GenerateCodeAsync(AgentRequest request)
    {
        _logger.LogInformation("Génération de code");

        // Claude donne les guidelines
        var guidance = await _claudeService.GenerateCodeSuggestionAsync(
            request.Message,
            request.ProjectContext);

        // GPT génère le code
        var generatedCode = await _gptService.GenerateCodeAsync(
            request.Message,
            guidance);

        var fileModifications = ExtractCodeFromResponse(generatedCode, request.FilePath);

        return new AgentResponse
        {
            ResponseText = $"💡 **Guidance Claude:**\n{guidance}\n\n🔧 **Code généré:**\n{generatedCode}",
            ModifiedFiles = fileModifications,
            Success = true
        };
    }

    private bool IsCodeGenerationRequired(string analysis, string userMessage)
    {
        var codeKeywords = new[]
        {
            "génère", "crée", "ajoute", "écris", "implemente", "développe",
            "méthode", "classe", "composant", "service", "contrôleur",
            "generate", "create", "add", "write", "implement", "develop"
        };

        return codeKeywords.Any(keyword =>
            analysis.ToLower().Contains(keyword) ||
            userMessage.ToLower().Contains(keyword));
    }

    private List<FileModification> ExtractCodeFromResponse(string response, string? filePath)
    {
        var modifications = new List<FileModification>();

        // Regex pour extraire les blocs de code C#
        var codeBlockRegex = new Regex(@"```(?:csharp|cs|c#)?\s*\n(.*?)\n```",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        var matches = codeBlockRegex.Matches(response);

        if (matches.Count > 0 && !string.IsNullOrEmpty(filePath))
        {
            var codeContent = matches[0].Groups[1].Value.Trim();

            modifications.Add(new FileModification
            {
                Path = filePath,
                NewContent = codeContent,
                ModificationType = "update",
                Diff = GenerateSimpleDiff(filePath, codeContent)
            });
        }

        return modifications;
    }

    private string? GenerateTestFilePath(string? originalFilePath)
    {
        if (string.IsNullOrEmpty(originalFilePath))
            return null;

        var directory = Path.GetDirectoryName(originalFilePath) ?? "";
        var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
        var extension = Path.GetExtension(originalFilePath);

        // Créer le chemin vers le projet de tests
        var testDirectory = directory.Replace("\\src\\", "\\tests\\")
                                   .Replace("/src/", "/tests/");

        return Path.Combine(testDirectory, $"{fileName}Tests{extension}");
    }

    private string GenerateSimpleDiff(string filePath, string newContent)
    {
        // Implémentation simplifiée du diff
        // Dans un vrai projet, tu pourrais utiliser une bibliothèque comme DiffPlex
        try
        {
            if (File.Exists(filePath))
            {
                var originalContent = File.ReadAllText(filePath);
                var originalLines = originalContent.Split('\n').Length;
                var newLines = newContent.Split('\n').Length;

                return $"Fichier modifié: {originalLines} lignes → {newLines} lignes";
            }
            else
            {
                return "Nouveau fichier créé";
            }
        }
        catch
        {
            return "Modification détectée";
        }
    }
}