# AI Agent pour Visual Studio 2022

## Structure créée automatiquement

Cette solution contient:
- **AIAgentMiddleware**: API middleware ASP.NET Core
- **AIAgentExtension**: Extension Visual Studio (à créer manuellement)

## Prochaines étapes:

1. **Configurer les API Keys:**
   ```
   cd AIAgentMiddleware
   dotnet user-secrets init
   dotnet user-secrets set "ApiKeys:Claude" "votre-clé-claude"
   dotnet user-secrets set "ApiKeys:OpenAI" "votre-clé-openai"
   ```

2. **Créer l'extension VS:**
   - Ouvrir Visual Studio 2022
   - File → New Project → VSIX Project
   - Nom: AIAgentExtension
   - Copier les fichiers fournis dans les artifacts

3. **Tester:**
   ```
   cd AIAgentMiddleware
   dotnet run
   ```

## Fichiers à copier:

Consultez les artifacts Claude pour obtenir tout le code source.

Bon développement! 🚀
