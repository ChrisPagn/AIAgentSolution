using Microsoft.AspNetCore.Mvc;
using AIAgentMiddleware.Models;
using AIAgentMiddleware.Services;

namespace AIAgentMiddleware.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentOrchestrator _orchestrator;
    private readonly ILogger<AgentController> _logger;

    public AgentController(IAgentOrchestrator orchestrator, ILogger<AgentController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<ActionResult<AgentResponse>> ProcessRequest([FromBody] AgentRequest request)
    {
        try
        {
            _logger.LogInformation("Traitement de la requ�te: {Message}", request.Message);

            var response = await _orchestrator.ProcessRequestAsync(request);

            _logger.LogInformation("R�ponse g�n�r�e avec {FileCount} modifications de fichiers",
                response.ModifiedFiles?.Count ?? 0);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement de la requ�te");
            return StatusCode(500, new AgentResponse
            {
                ResponseText = $"Erreur interne: {ex.Message}",
                ModifiedFiles = new List<FileModification>()
            });
        }
    }

    [HttpPost("analyze-code")]
    public async Task<ActionResult<AgentResponse>> AnalyzeCode([FromBody] CodeAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Analyse de code pour: {FilePath}", request.FilePath);

            var agentRequest = new AgentRequest
            {
                Message = $"Analyse ce code et donne-moi des suggestions d'am�lioration:\n\n```csharp\n{request.Code}\n```",
                ProjectContext = request.ProjectContext,
                Instruction = "analyze-code",
                FilePath = request.FilePath
            };

            var response = await _orchestrator.ProcessRequestAsync(agentRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse du code");
            return StatusCode(500, new AgentResponse
            {
                ResponseText = $"Erreur lors de l'analyse: {ex.Message}",
                ModifiedFiles = new List<FileModification>()
            });
        }
    }

    [HttpPost("refactor")]
    public async Task<ActionResult<AgentResponse>> RefactorCode([FromBody] RefactorRequest request)
    {
        try
        {
            _logger.LogInformation("Refactoring demand�: {RefactorType}", request.RefactorType);

            var agentRequest = new AgentRequest
            {
                Message = $"Refactorise ce code ({request.RefactorType}):\n\n```csharp\n{request.Code}\n```\n\nInstructions sp�cifiques: {request.Instructions}",
                ProjectContext = request.ProjectContext,
                Instruction = "refactor",
                FilePath = request.FilePath
            };

            var response = await _orchestrator.ProcessRequestAsync(agentRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du refactoring");
            return StatusCode(500, new AgentResponse
            {
                ResponseText = $"Erreur lors du refactoring: {ex.Message}",
                ModifiedFiles = new List<FileModification>()
            });
        }
    }

    [HttpPost("generate-tests")]
    public async Task<ActionResult<AgentResponse>> GenerateTests([FromBody] TestGenerationRequest request)
    {
        try
        {
            _logger.LogInformation("G�n�ration de tests pour: {ClassName}", request.ClassName);

            var agentRequest = new AgentRequest
            {
                Message = $"G�n�re des tests unitaires complets pour cette classe:\n\n```csharp\n{request.Code}\n```\n\nFramework de test: {request.TestFramework}",
                ProjectContext = request.ProjectContext,
                Instruction = "generate-tests",
                FilePath = request.FilePath
            };

            var response = await _orchestrator.ProcessRequestAsync(agentRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la g�n�ration des tests");
            return StatusCode(500, new AgentResponse
            {
                ResponseText = $"Erreur lors de la g�n�ration des tests: {ex.Message}",
                ModifiedFiles = new List<FileModification>()
            });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
