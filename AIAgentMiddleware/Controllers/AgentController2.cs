//using Microsoft.AspNetCore.Mvc;
//using AIAgentMiddleware.Models;
//using AIAgentMiddleware.Services;

//namespace AIAgentMiddleware.Controllers;

////[ApiController]
////[Route("api/[controller]")]
//public class AgentController2 : ControllerBase
//{
//    private readonly IAgentOrchestrator _orchestrator;
//    private readonly ILogger<AgentController> _logger;

//    public AgentController2(IAgentOrchestrator orchestrator, ILogger<AgentController> logger)
//    {
//        _orchestrator = orchestrator;
//        _logger = logger;
//    }

//    [HttpPost("process")]
//    public async Task<ActionResult<AgentResponse>> ProcessRequest([FromBody] AgentRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Traitement de la requête: {Message}", request.Message);

//            var response = await _orchestrator.ProcessRequestAsync(request);

//            _logger.LogInformation("Réponse générée avec {FileCount} modifications de fichiers",
//                response.ModifiedFiles?.Count ?? 0);

//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erreur lors du traitement de la requête");
//            return StatusCode(500, new AgentResponse
//            {
//                ResponseText = $"Erreur interne: {ex.Message}",
//                ModifiedFiles = new List<FileModification>(),
//                Success = false,
//                ErrorMessage = ex.Message
//            });
//        }
//    }

//    [HttpPost("analyze-code")]
//    public async Task<ActionResult<AgentResponse>> AnalyzeCode([FromBody] CodeAnalysisRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Analyse de code pour: {FilePath}", request.FilePath);

//            var agentRequest = new AgentRequest
//            {
//                Message = $"Analyse ce code et donne-moi des suggestions d'amélioration:\n\n```csharp\n{request.Code}\n```",
//                ProjectContext = request.ProjectContext,
//                Instruction = "analyze-code",
//                FilePath = request.FilePath,
//                SelectedCode = request.Code
//            };

//            var response = await _orchestrator.ProcessRequestAsync(agentRequest);
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erreur lors de l'analyse du code");
//            return StatusCode(500, new AgentResponse
//            {
//                ResponseText = $"Erreur lors de l'analyse: {ex.Message}",
//                ModifiedFiles = new List<FileModification>(),
//                Success = false,
//                ErrorMessage = ex.Message
//            });
//        }
//    }

//    [HttpPost("refactor")]
//    public async Task<ActionResult<AgentResponse>> RefactorCode([FromBody] RefactorRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Refactoring demandé: {RefactorType}", request.RefactorType);

//            var agentRequest = new AgentRequest
//            {
//                Message = $"Refactorise ce code ({request.RefactorType}):\n\n```csharp\n{request.Code}\n```\n\nInstructions spécifiques: {request.Instructions}",
//                ProjectContext = request.ProjectContext,
//                Instruction = "refactor",
//                FilePath = request.FilePath,
//                SelectedCode = request.Code
//            };

//            var response = await _orchestrator.ProcessRequestAsync(agentRequest);
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erreur lors du refactoring");
//            return StatusCode(500, new AgentResponse
//            {
//                ResponseText = $"Erreur lors du refactoring: {ex.Message}",
//                ModifiedFiles = new List<FileModification>(),
//                Success = false,
//                ErrorMessage = ex.Message
//            });
//        }
//    }

//    [HttpPost("generate-tests")]
//    public async Task<ActionResult<AgentResponse>> GenerateTests([FromBody] TestGenerationRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Génération de tests pour: {ClassName}", request.ClassName);

//            var agentRequest = new AgentRequest
//            {
//                Message = $"Génère des tests unitaires complets pour cette classe:\n\n```csharp\n{request.Code}\n```\n\nFramework de test: {request.TestFramework}",
//                ProjectContext = request.ProjectContext,
//                Instruction = "generate-tests",
//                FilePath = request.FilePath,
//                SelectedCode = request.Code
//            };

//            var response = await _orchestrator.ProcessRequestAsync(agentRequest);
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erreur lors de la génération des tests");
//            return StatusCode(500, new AgentResponse
//            {
//                ResponseText = $"Erreur lors de la génération des tests: {ex.Message}",
//                ModifiedFiles = new List<FileModification>(),
//                Success = false,
//                ErrorMessage = ex.Message
//            });
//        }
//    }

//    [HttpGet("health")]
//    public IActionResult Health()
//    {
//        return Ok(new
//        {
//            Status = "Healthy",
//            Timestamp = DateTime.UtcNow,
//            Version = "1.0.0",
//            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
//        });
//    }

//    [HttpGet("test-config")]
//    public IActionResult TestConfiguration([FromServices] IConfiguration configuration)
//    {
//        var claudeKey = configuration["ApiKeys:Claude"];
//        var openAiKey = configuration["ApiKeys:OpenAI"];

//        return Ok(new
//        {
//            ClaudeConfigured = !string.IsNullOrEmpty(claudeKey),
//            OpenAIConfigured = !string.IsNullOrEmpty(openAiKey),
//            ClaudeKeyPrefix = claudeKey?.Substring(0, Math.Min(15, claudeKey.Length)) + "...",
//            OpenAIKeyPrefix = openAiKey?.Substring(0, Math.Min(10, openAiKey.Length)) + "..."
//        });
//    }
//}