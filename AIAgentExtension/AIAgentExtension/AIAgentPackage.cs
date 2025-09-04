using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace AIAgentExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(AIAgentPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(AIAgentToolWindow))]
    [ProvideOptionPage(typeof(OptionsPage), "AI Agent", "General", 0, 0, true)]
    public sealed class AIAgentPackage : AsyncPackage
    {
        public const string PackageGuidString = "12345678-1234-1234-1234-123456789012";
        public static readonly Guid CommandSetGuid = new Guid("87654321-4321-4321-4321-210987654321");

        public const int ShowToolWindowCommandId = 0x0100;
        public const int AnalyzeCodeCommandId = 0x0101;
        public const int RefactorCommandId = 0x0102;
        public const int GenerateTestsCommandId = 0x0103;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Enregistrer les commandes
            await ShowToolWindowCommand.InitializeAsync(this);
            await AnalyzeCodeCommand.InitializeAsync(this);
            await RefactorCommand.InitializeAsync(this);
            await GenerateTestsCommand.InitializeAsync(this);
        }

        public async Task<IVsWindowFrame> ShowToolWindowAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var window = FindToolWindow(typeof(AIAgentToolWindow), 0, true);
            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            return windowFrame;
        }
    }

    // Page d'options pour configurer les API keys
    public class OptionsPage : DialogPage
    {
        [Category("API Configuration")]
        [DisplayName("Claude API Key")]
        [Description("Clé API pour Anthropic Claude")]
        public string ClaudeApiKey { get; set; } = string.Empty;

        [Category("API Configuration")]
        [DisplayName("OpenAI API Key")]
        [Description("Clé API pour OpenAI GPT")]
        public string OpenAIApiKey { get; set; } = string.Empty;

        [Category("Middleware")]
        [DisplayName("Agent API URL")]
        [Description("URL de votre middleware agent")]
        public string AgentApiUrl { get; set; } = "http://localhost:5210"; // Corrigé le port
    }
}