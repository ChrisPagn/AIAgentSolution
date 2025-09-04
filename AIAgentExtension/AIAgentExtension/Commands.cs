using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using EnvDTE;

namespace AIAgentExtension
{
    // Commande pour ouvrir la fen�tre de chat
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

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

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

    // Commande pour analyser le code s�lectionn�
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
                    // Ouvrir la fen�tre de chat avec un message pr�-rempli
                    var package = this.package as AIAgentPackage;
                    await package?.ShowToolWindowAsync();

                    // TODO: Envoyer automatiquement le code s�lectionn� pour analyse
                    // Cela n�cessiterait une r�f�rence � AIAgentControl ou un syst�me d'�v�nements
                }
                else
                {
                    VsShellUtilities.ShowMessageBox(
                        this.package,
                        "Veuillez s�lectionner du code � analyser.",
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

            var package = this.package as AIAgentPackage;
            await package?.ShowToolWindowAsync();

            // TODO: Pr�-remplir avec "Refactore ce code :"
        }
    }

    // Commande pour g�n�rer des tests
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

            var package = this.package as AIAgentPackage;
            await package?.ShowToolWindowAsync();

            // TODO: Pr�-remplir avec "G�n�re des tests unitaires pour :"
        }
    }
}