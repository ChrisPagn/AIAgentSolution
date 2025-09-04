namespace AIAgentMiddleware.Models;

// Requête principale de l'agent
public class AgentRequest
{
	public string Message { get; set; } = string.Empty;
	public string ProjectContext { get; set; } = string.Empty;
	public string Instruction { get; set; } = string.Empty;
	public string? FilePath { get; set; }
	public string? SelectedCode { get; set; }
	public Dictionary<string, object>? Metadata { get; set; }
}

// Réponse de l'agent
public class AgentResponse
{
	public string ResponseText { get; set; } = string.Empty;
	public List<FileModification> ModifiedFiles { get; set; } = new();
	public string? Explanation { get; set; }
	public List<string>? Suggestions { get; set; }
	public bool Success { get; set; } = true;
	public string? ErrorMessage { get; set; }
}

// Modification de fichier
public class FileModification
{
	public string Path { get; set; } = string.Empty;
	public string? Diff { get; set; }
	public string NewContent { get; set; } = string.Empty;
	public string ModificationType { get; set; } = "update"; // update, create, delete
	public string? BackupContent { get; set; }
}

// Requête d'analyse de code
public class CodeAnalysisRequest
{
	public string Code { get; set; } = string.Empty;
	public string ProjectContext { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public string? AnalysisType { get; set; } = "general"; // general, performance, security, etc.
}

// Requête de refactoring
public class RefactorRequest
{
	public string Code { get; set; } = string.Empty;
	public string ProjectContext { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public string RefactorType { get; set; } = string.Empty; // extract-method, dependency-injection, etc.
	public string? Instructions { get; set; }
}

// Requête de génération de tests
public class TestGenerationRequest
{
	public string Code { get; set; } = string.Empty;
	public string ProjectContext { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public string ClassName { get; set; } = string.Empty;
	public string TestFramework { get; set; } = "xUnit"; // xUnit, NUnit, MSTest
	public bool IncludeMockingFramework { get; set; } = true;
	public string? MockingFramework { get; set; } = "Moq";
}

// Modèles pour les APIs LLM
public class ClaudeRequest
{
	public string Model { get; set; } = "claude-3-5-sonnet-20241022";
	public List<ClaudeMessage> Messages { get; set; } = new();
	public int MaxTokens { get; set; } = 4000;
	public double Temperature { get; set; } = 0.3;
	public List<string>? StopSequences { get; set; }
}

public class ClaudeMessage
{
	public string Role { get; set; } = string.Empty; // user, assistant
	public string Content { get; set; } = string.Empty;
}

public class ClaudeResponse
{
	public string Id { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public List<ClaudeContent> Content { get; set; } = new();
	public string Model { get; set; } = string.Empty;
	public ClaudeUsage? Usage { get; set; }
}

public class ClaudeContent
{
	public string Type { get; set; } = string.Empty;
	public string Text { get; set; } = string.Empty;
}

public class ClaudeUsage
{
	public int InputTokens { get; set; }
	public int OutputTokens { get; set; }
}

// Modèles OpenAI
public class OpenAIRequest
{
	public string Model { get; set; } = "gpt-4";
	public List<OpenAIMessage> Messages { get; set; } = new();
	public int MaxTokens { get; set; } = 4000;
	public double Temperature { get; set; } = 0.3;
	public List<string>? Stop { get; set; }
}

public class OpenAIMessage
{
	public string Role { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
}

public class OpenAIResponse
{
	public string Id { get; set; } = string.Empty;
	public string Object { get; set; } = string.Empty;
	public long Created { get; set; }
	public string Model { get; set; } = string.Empty;
	public List<OpenAIChoice> Choices { get; set; } = new();
	public OpenAIUsage? Usage { get; set; }
}

public class OpenAIChoice
{
	public int Index { get; set; }
	public OpenAIMessage Message { get; set; } = new();
	public string FinishReason { get; set; } = string.Empty;
}

public class OpenAIUsage
{
	public int PromptTokens { get; set; }
	public int CompletionTokens { get; set; }
	public int TotalTokens { get; set; }
}