using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using System.IO;
using System.Text;

namespace AIAgentExtension
{
    // Commande pour ouvrir la fenêtre de chat
    internal sealed class ShowToolWindowCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("87654321-4321-4321-4321-210987654321");

        private readonly AsyncPackage package;

        private ShowToolWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ShowToolWindowCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ShowToolWindowCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var package = this.package as AIAgentPackage;
            _ = package?.ShowToolWindowAsync();
        }
    }

    // Commande pour analyser le code sélectionné
    internal sealed class AnalyzeCodeCommand
    {
        public const int CommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("87654321-4321-4321-4321-210987654321");

        private readonly AsyncPackage package;

        private AnalyzeCodeCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static AnalyzeCodeCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AnalyzeCodeCommand(package, commandService);
        }

        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                var dte = await ServiceProvider.GetGlobalServiceAsync(typeof(DTE)) as DTE;
                if (dte?.ActiveDocument?.Selection is TextSelection selection && !string.IsNullOrEmpty(selection.Text))
                {
                    // Analyser le code sélectionné
                    await AnalyzeSelectedCodeAsync(selection.Text, dte);
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(
                        this.package,
                        "Veuillez sélectionner du code à analyser.",
                        "AI Agent",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Erreur lors de l'analyse : {ex.Message}",
                    "AI Agent Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private async Task AnalyzeSelectedCodeAsync(string selectedCode, DTE dte)
        {
            // Ouvrir la fenêtre de chat et envoyer automatiquement la demande d'analyse
            var package = this.package as AIAgentPackage;
            await package?.ShowToolWindowAsync();

            // Optionnel : envoyer automatiquement l'analyse
            // TODO: Ajouter la logique pour envoyer automatiquement à l'agent
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;
    }

    // Commande pour refactoriser
    internal sealed class RefactorCommand
    {
        public const int CommandId = 0x0102;
        public static readonly Guid CommandSet = new Guid("87654321-4321-4321-4321-210987654321");

        private readonly AsyncPackage package;

        private RefactorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static RefactorCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RefactorCommand(package, commandService);
        }

        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                var dte = await ServiceProvider.GetGlobalServiceAsync(typeof(DTE)) as DTE;
                if (dte?.ActiveDocument?.Selection is TextSelection selection && !string.IsNullOrEmpty(selection.Text))
                {
                    await RefactorSelectedCodeAsync(selection.Text, dte);
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(
                        this.package,
                        "Veuillez sélectionner du code à refactoriser.",
                        "AI Agent",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Erreur lors du refactoring : {ex.Message}",
                    "AI Agent Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private async Task RefactorSelectedCodeAsync(string selectedCode, DTE dte)
        {
            var package = this.package as AIAgentPackage;
            await package?.ShowToolWindowAsync();
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;
    }

    // Commande pour générer des tests
    internal sealed class GenerateTestsCommand
    {
        public const int CommandId = 0x0103;
        public static readonly Guid CommandSet = new Guid("87654321-4321-4321-4321-210987654321");

        private readonly AsyncPackage package;

        private GenerateTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static GenerateTestsCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GenerateTestsCommand(package, commandService);
        }

        private async void Execute(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                var dte = await ServiceProvider.GetGlobalServiceAsync(typeof(DTE)) as DTE;
                if (dte?.ActiveDocument?.Selection is TextSelection selection && !string.IsNullOrEmpty(selection.Text))
                {
                    await GenerateTestsForCodeAsync(selection.Text, dte);
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(
                        this.package,
                        "Veuillez sélectionner du code pour générer des tests.",
                        "AI Agent",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Erreur lors de la génération des tests : {ex.Message}",
                    "AI Agent Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private async Task GenerateTestsForCodeAsync(string selectedCode, DTE dte)
        {
            var package = this.package as AIAgentPackage;
            await package?.ShowToolWindowAsync();
        }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;
    }
}