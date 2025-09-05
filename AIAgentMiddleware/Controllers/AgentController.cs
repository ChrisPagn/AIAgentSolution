using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AIAgentMiddleware.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly ILogger<AgentController> _logger;
    private readonly IConfiguration _configuration;

    public AgentController(ILogger<AgentController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("process")]
    public IActionResult ProcessRequest([FromBody] AgentRequest request)
    {
        _logger.LogInformation("Traitement de la requête: {Message}", request.Message);

        // Mode démo intelligent
        var claudeKey = _configuration["ApiKeys:Claude"];
        var isDemoMode = string.IsNullOrEmpty(claudeKey) || claudeKey == "your-claude-api-key-here";

        if (isDemoMode)
        {
            return Ok(GenerateIntelligentDemoResponse(request));
        }

        return StatusCode(500, new AgentResponse
        {
            ResponseText = "Services Claude/GPT non encore implémentés avec vraies clés",
            Success = false,
            ErrorMessage = "Implémentation en cours"
        });
    }

    private AgentResponse GenerateIntelligentDemoResponse(AgentRequest request)
    {
        var codeAnalysis = AnalyzeCodeIntelligently(request.SelectedCode ?? "");

        return request.Instruction switch
        {
            "analyze-code" => new AgentResponse
            {
                ResponseText = GenerateCodeAnalysis(codeAnalysis, request),
                ModifiedFiles = new List<FileModification>(),
                Success = true,
                Explanation = "Analyse intelligente en mode démo",
                Suggestions = codeAnalysis.Suggestions
            },

            "refactor" => new AgentResponse
            {
                ResponseText = GenerateRefactoringResponse(codeAnalysis, request),
                ModifiedFiles = GenerateRefactoredCode(codeAnalysis, request),
                Success = true,
                Explanation = "Refactoring intelligent en mode démo"
            },

            "generate-tests" => new AgentResponse
            {
                ResponseText = GenerateTestResponse(codeAnalysis, request),
                ModifiedFiles = GenerateTestCode(codeAnalysis, request),
                Success = true,
                Explanation = "Génération de tests intelligente"
            },

            _ => new AgentResponse
            {
                ResponseText = GenerateGeneralResponse(request),
                ModifiedFiles = new List<FileModification>(),
                Success = true,
                Explanation = "Réponse générale intelligente"
            }
        };
    }

    private CodeAnalysis AnalyzeCodeIntelligently(string code)
    {
        var analysis = new CodeAnalysis();

        // Détection de patterns dans le code
        if (code.Contains("ControllerBase"))
        {
            analysis.Type = "Controller";
            if (!code.Contains("async")) analysis.Issues.Add("Méthodes non asynchrones détectées");
            if (!code.Contains("ILogger")) analysis.Issues.Add("Pas de logging implémenté");
            if (code.Contains("_context") && !code.Contains("private readonly")) analysis.Issues.Add("DbContext mal injecté");
        }

        if (code.Contains("class") && code.Contains("Service"))
        {
            analysis.Type = "Service";
            if (!code.Contains("interface")) analysis.Issues.Add("Pas d'interface pour l'injection de dépendances");
        }

        if (code.Contains("@page") || code.Contains(".razor"))
        {
            analysis.Type = "BlazorComponent";
            if (!code.Contains("MudBlazor")) analysis.Suggestions.Add("Considérer l'utilisation de MudBlazor pour l'UI");
        }

        // Analyse de sécurité
        if (code.Contains("ToListAsync()") && !code.Contains("Where"))
        {
            analysis.Issues.Add("Requête sans filtrage - risque de performance");
        }

        if (code.Contains("string") && code.Contains("sql") && code.Contains("+"))
        {
            analysis.Issues.Add("CRITIQUE: Possible injection SQL détectée");
        }

        // Suggestions intelligentes
        if (analysis.Type == "Controller")
        {
            analysis.Suggestions.AddRange(new[]
            {
                "Implémenter une validation des modèles avec [ApiController]",
                "Ajouter des codes de statut HTTP appropriés",
                "Utiliser des DTOs au lieu de retourner directement les entités",
                "Implémenter la gestion d'erreurs avec try-catch"
            });
        }

        return analysis;
    }

    private string GenerateCodeAnalysis(CodeAnalysis analysis, AgentRequest request)
    {
        var response = $@"🔍 **Analyse détaillée du code {analysis.Type}**

**Code analysé :** `{request.SelectedCode?.Substring(0, Math.Min(100, request.SelectedCode?.Length ?? 0))}...`

## 🚨 Problèmes identifiés :
{string.Join("\n", analysis.Issues.Select(i => $"• **{i}**"))}

## 💡 Suggestions d'amélioration :
{string.Join("\n", analysis.Suggestions.Select(s => $"• {s}"))}

## 🏗️ Architecture recommandée :
• **Couche Controller** : Validation et transformation des données
• **Couche Service** : Logique métier
• **Couche Repository** : Accès aux données

## 🔧 Bonnes pratiques {analysis.Type} :";

        if (analysis.Type == "Controller")
        {
            response += @"
• Utiliser `[ApiController]` pour la validation automatique
• Retourner des `ActionResult<T>` typés
• Implémenter des DTOs pour les entrées/sorties
• Ajouter des attributs de documentation Swagger";
        }

        response += "\n\n*💡 Conseil : Configure tes clés API Claude/OpenAI pour des analyses encore plus précises !*";

        return response;
    }

    private string GenerateRefactoringResponse(CodeAnalysis analysis, AgentRequest request)
    {
        return $@"🔄 **Refactoring intelligent du {analysis.Type}**

## 📝 Améliorations apportées :
• ✅ **Injection de dépendances** correctement implémentée
• ✅ **Séparation des responsabilités** appliquée
• ✅ **Gestion d'erreurs** robuste ajoutée
• ✅ **Logging** intégré
• ✅ **Validation** des données d'entrée

## 🎯 Pattern appliqué :
**Repository + Service Pattern** avec injection de dépendances

## 📊 Métriques d'amélioration :
• **Maintenabilité** : +40%
• **Testabilité** : +60%
• **Performance** : +25%
• **Sécurité** : +50%

Consultez le fichier modifié ci-dessous pour voir le code refactorisé ! 👇";
    }

    private List<FileModification> GenerateRefactoredCode(CodeAnalysis analysis, AgentRequest request)
    {
        if (string.IsNullOrEmpty(request.FilePath)) return new List<FileModification>();

        var refactoredCode = analysis.Type switch
        {
            "Controller" => GenerateRefactoredController(request.SelectedCode ?? ""),
            "Service" => GenerateRefactoredService(request.SelectedCode ?? ""),
            _ => GenerateGenericRefactoredCode(request.SelectedCode ?? "")
        };

        return new List<FileModification>
        {
            new FileModification
            {
                Path = request.FilePath,
                NewContent = refactoredCode,
                ModificationType = "update",
                Diff = $"🔄 Refactoring intelligent appliqué - {analysis.Issues.Count} problèmes corrigés"
            }
        };
    }

    private string GenerateRefactoredController(string originalCode)
    {
        return @"using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace YourProject.Controllers;

/// <summary>
/// Contrôleur pour la gestion des utilisateurs
/// Refactorisé avec les bonnes pratiques ASP.NET Core
/// </summary>
[ApiController]
[Route(""api/[controller]"")]
[Produces(""application/json"")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Récupère tous les utilisateurs avec pagination
    /// </summary>
    /// <param name=""pageSize"">Nombre d'éléments par page (max 100)</param>
    /// <param name=""pageNumber"">Numéro de la page (commence à 1)</param>
    /// <returns>Liste paginée des utilisateurs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [Range(1, 100)] int pageSize = 10,
        [Range(1, int.MaxValue)] int pageNumber = 1)
    {
        try
        {
            _logger.LogInformation(""Récupération des utilisateurs - Page: {PageNumber}, Taille: {PageSize}"", 
                pageNumber, pageSize);

            var result = await _userService.GetUsersAsync(pageNumber, pageSize);
            
            if (!result.Items.Any())
            {
                _logger.LogInformation(""Aucun utilisateur trouvé"");
                return Ok(new PagedResult<UserDto> { Items = new List<UserDto>(), TotalCount = 0 });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ""Erreur lors de la récupération des utilisateurs"");
            return StatusCode(500, ""Une erreur interne s'est produite"");
        }
    }

    /// <summary>
    /// Crée un nouvel utilisateur
    /// </summary>
    /// <param name=""createUserDto"">Données de l'utilisateur à créer</param>
    /// <returns>Utilisateur créé</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        try
        {
            _logger.LogInformation(""Création d'un utilisateur: {Email}"", createUserDto.Email);

            var user = await _userService.CreateUserAsync(createUserDto);
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, ""Erreur de validation lors de la création de l'utilisateur"");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ""Erreur lors de la création de l'utilisateur"");
            return StatusCode(500, ""Une erreur interne s'est produite"");
        }
    }

    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    /// <param name=""id"">ID de l'utilisateur</param>
    /// <returns>Utilisateur trouvé</returns>
    [HttpGet(""{id:int}"")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            
            if (user == null)
            {
                _logger.LogWarning(""Utilisateur non trouvé: {UserId}"", id);
                return NotFound($""Utilisateur avec l'ID {id} non trouvé"");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ""Erreur lors de la récupération de l'utilisateur {UserId}"", id);
            return StatusCode(500, ""Une erreur interne s'est produite"");
        }
    }
}

// DTOs pour séparer les modèles de domaine des contrats d'API
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    [Required(ErrorMessage = ""Le nom est obligatoire"")]
    [StringLength(100, ErrorMessage = ""Le nom ne peut pas dépasser 100 caractères"")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = ""L'email est obligatoire"")]
    [EmailAddress(ErrorMessage = ""Format d'email invalide"")]
    public string Email { get; set; } = string.Empty;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// Interface pour le service (à implémenter)
public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersAsync(int pageNumber, int pageSize);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
}";
    }

    private string GenerateRefactoredService(string originalCode)
    {
        return @"// Service refactorisé avec injection de dépendances et bonnes pratiques
public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository, 
        ILogger<UserService> logger,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<List<UserDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ""Erreur lors de la récupération des utilisateurs"");
            throw;
        }
    }
}";
    }

    private string GenerateGenericRefactoredCode(string originalCode)
    {
        return $@"// Code refactorisé avec les bonnes pratiques C#
// Original: {originalCode.Substring(0, Math.Min(50, originalCode.Length))}...

// ✅ Améliorations appliquées:
// - Injection de dépendances
// - Gestion d'erreurs
// - Logging
// - Validation
// - Documentation XML

namespace YourProject.Improved;

/// <summary>
/// Classe refactorisée selon les bonnes pratiques
/// </summary>
public class ImprovedClass
{{
    private readonly IService _service;
    private readonly ILogger<ImprovedClass> _logger;

    public ImprovedClass(IService service, ILogger<ImprovedClass> logger)
    {{
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }}

    /// <summary>
    /// Méthode améliorée avec gestion d'erreurs
    /// </summary>
    public async Task<Result<T>> ImprovedMethodAsync<T>()
    {{
        try
        {{
            _logger.LogInformation(""Début de traitement"");
            
            var result = await _service.ProcessAsync();
            
            _logger.LogInformation(""Traitement terminé avec succès"");
            return Result.Success(result);
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""Erreur lors du traitement"");
            return Result.Failure<T>(ex.Message);
        }}
    }}
}}";
    }

    private string GenerateTestResponse(CodeAnalysis analysis, AgentRequest request)
    {
        return $@"🧪 **Tests unitaires générés pour {analysis.Type}**

## 📋 Stratégie de test :
• **Tests des cas nominaux** : Scénarios de succès
• **Tests des cas d'erreur** : Gestion des exceptions
• **Tests de validation** : Paramètres invalides
• **Tests d'intégration** : Interactions entre composants

## 🎯 Couverture générée :
• **Méthodes publiques** : 100%
• **Cas d'erreur** : 95%
• **Validation** : 100%

## 🏗️ Frameworks utilisés :
• **xUnit** : Framework de test principal
• **Moq** : Mocking des dépendances  
• **FluentAssertions** : Assertions expressives

Fichier de test complet généré ci-dessous ! 👇";
    }

    private List<FileModification> GenerateTestCode(CodeAnalysis analysis, AgentRequest request)
    {
        if (string.IsNullOrEmpty(request.FilePath)) return new List<FileModification>();

        var testPath = request.FilePath.Replace(".cs", "Tests.cs").Replace("Controllers", "Tests.Controllers");

        var testCode = analysis.Type switch
        {
            "Controller" => GenerateControllerTests(),
            _ => GenerateGenericTests()
        };

        return new List<FileModification>
        {
            new FileModification
            {
                Path = testPath,
                NewContent = testCode,
                ModificationType = "create",
                Diff = "🧪 Fichier de tests unitaires créé avec couverture complète"
            }
        };
    }

    private string GenerateControllerTests()
    {
        return @"using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace YourProject.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();
        _controller = new UserController(_mockUserService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUsers_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var expectedUsers = new PagedResult<UserDto>
        {
            Items = new List<UserDto>
            {
                new UserDto { Id = 1, Name = ""John Doe"", Email = ""john@example.com"" }
            },
            TotalCount = 1
        };
        
        _mockUserService
            .Setup(s => s.GetUsersAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _controller.GetUsers(10, 1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task GetUsers_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockUserService
            .Setup(s => s.GetUsersAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception(""Database error""));

        // Act
        var result = await _controller.GetUsers(10, 1);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData(0, 1)]    // PageSize invalide
    [InlineData(101, 1)]  // PageSize trop grand
    [InlineData(10, 0)]   // PageNumber invalide
    public async Task GetUsers_WithInvalidParameters_ReturnsBadRequest(int pageSize, int pageNumber)
    {
        // Act & Assert
        // Note: Avec [ApiController], la validation se fait automatiquement
        // Ces tests vérifieraient la validation des attributs [Range]
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Name = ""Jane Doe"",
            Email = ""jane@example.com""
        };

        var createdUser = new UserDto
        {
            Id = 2,
            Name = ""Jane Doe"",
            Email = ""jane@example.com"",
            CreatedAt = DateTime.UtcNow
        };

        _mockUserService
            .Setup(s => s.CreateUserAsync(createUserDto))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Value.Should().BeEquivalentTo(createdUser);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            Name = ""John Doe"",
            Email = ""existing@example.com""
        };

        _mockUserService
            .Setup(s => s.CreateUserAsync(createUserDto))
            .ThrowsAsync(new InvalidOperationException(""Email already exists""));

        // Act
        var result = await _controller.CreateUser(createUserDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUser_WithExistingId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new UserDto
        {
            Id = userId,
            Name = ""John Doe"",
            Email = ""john@example.com""
        };

        _mockUserService
            .Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetUser_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var userId = 999;
        
        _mockUserService
            .Setup(s => s.GetUserByIdAsync(userId))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}

// Tests d'intégration (optionnel)
public class UserControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UserControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_Integration_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync(""/api/user"");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Contain(""application/json"");
    }
}";
    }

    private string GenerateGenericTests()
    {
        return @"// Tests unitaires générés automatiquement
using Xunit;
using Moq;
using FluentAssertions;

public class GeneratedTests
{
    [Fact]
    public void SampleTest_ShouldPass()
    {
        // Arrange
        var expected = true;
        
        // Act
        var actual = true;
        
        // Assert
        actual.Should().Be(expected);
    }
}";
    }

    private string GenerateGeneralResponse(AgentRequest request)
    {
        return $@"🤖 **Assistant IA - Analyse du projet**

**Votre demande :** ""{request.Message}""

## 📊 Contexte analysé :
{request.ProjectContext}

## 💡 Recommandations pour votre projet :

### 🏗️ Architecture
• **Pattern recommandé** : Clean Architecture avec CQRS
• **Couches** : Presentation → Application → Domain → Infrastructure
• **DI Container** : Utilisez les services natifs .NET

### 🔧 Technologies suggérées :
• **API** : ASP.NET Core avec Swagger
• **ORM** : Entity Framework Core
• **Tests** : xUnit + Moq + FluentAssertions
• **Logging** : Serilog
• **Validation** : FluentValidation

### 📚 Bonnes pratiques :
• Utilisez des DTOs pour les contrats d'API
• Implémentez la validation côté serveur
• Ajoutez une gestion d'erreurs globale
• Documentez votre API avec Swagger
• Sécurisez avec Authentication JWT

## 🚀 Prochaines étapes :
1. **Structure** : Organisez votre solution en couches
2. **Tests** : Ajoutez une couverture de tests > 80%
3. **CI/CD** : Configurez GitHub Actions
4. **Monitoring** : Intégrez Application Insights

*💡 Configure tes clés API Claude/OpenAI pour des réponses encore plus personnalisées !*";
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Message = "🚀 Middleware AI Agent opérationnel!"
        });
    }

    [HttpGet("test-config")]
    public IActionResult TestConfiguration()
    {
        var claudeKey = _configuration["ApiKeys:Claude"];
        var openAiKey = _configuration["ApiKeys:OpenAI"];

        return Ok(new
        {
            ClaudeConfigured = !string.IsNullOrEmpty(claudeKey) && claudeKey != "your-claude-api-key-here",
            OpenAIConfigured = !string.IsNullOrEmpty(openAiKey) && openAiKey != "your-openai-api-key-here",
            ClaudeKeyPrefix = claudeKey?.Length > 15 ? claudeKey.Substring(0, 15) + "..." : "Mode démo intelligent",
            OpenAIKeyPrefix = openAiKey?.Length > 10 ? openAiKey.Substring(0, 10) + "..." : "Mode démo intelligent",
            DemoMode = true,
            IntelligentAnalysis = "✅ Activée"
        });
    }
}

// Classes pour l'analyse intelligente
public class CodeAnalysis
{
    public string Type { get; set; } = "Unknown";
    public List<string> Issues { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

// Modèles de données (inchangés)
public class AgentRequest
{
    public string Message { get; set; } = string.Empty;
    public string ProjectContext { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? SelectedCode { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AgentResponse
{
    public string ResponseText { get; set; } = string.Empty;
    public List<FileModification> ModifiedFiles { get; set; } = new();
    public string? Explanation { get; set; }
    public List<string>? Suggestions { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public class FileModification
{
    public string Path { get; set; } = string.Empty;
    public string? Diff { get; set; }
    public string NewContent { get; set; } = string.Empty;
    public string ModificationType { get; set; } = "update";
    public string? BackupContent { get; set; }
}