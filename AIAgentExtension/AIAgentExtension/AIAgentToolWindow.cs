using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;


namespace AIAgentExtension
{
    [Guid("87654321-1111-1111-1111-111111111111")]
    public class AIAgentToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIAgentToolWindow"/> class,  representing a tool window for
        /// interacting with the AI Agent.
        /// </summary>
        /// <remarks>This tool window provides a user interface for AI Agent chat functionality.  The
        /// window's caption is set to "AI Agent Chat," and it contains an instance  of <see cref="AIAgentControl"/> as
        /// its content.</remarks>
        public AIAgentToolWindow() : base(null)
        {
            this.Caption = "AI Agent Chat";
            this.Content = new AIAgentControl();
        }
    }

    public partial class AIAgentControl : UserControl, INotifyPropertyChanged
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private ObservableCollection<ChatMessage> _messages;
        private string _inputText;
        private bool _isLoading;

        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public AIAgentControl()
        {
            InitializeComponent();
            Messages = new ObservableCollection<ChatMessage>();
            DataContext = this;

            // Message de bienvenue
            Messages.Add(new ChatMessage
            {
                IsUser = false,
                Text = "🤖 Salut ! Je suis ton agent IA. Je peux t'aider à :\n• Analyser ton code\n• Refactoriser des classes\n• Générer des tests\n• Créer de nouvelles méthodes\n\nQue puis-je faire pour toi ?",
                Timestamp = DateTime.Now
            });
        }

        private void InitializeComponent()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Zone de chat
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            var messagesListBox = new ListBox();
            messagesListBox.SetBinding(ListBox.ItemsSourceProperty, new System.Windows.Data.Binding("Messages"));
            messagesListBox.ItemTemplate = CreateMessageTemplate();

            scrollViewer.Content = messagesListBox;
            Grid.SetRow(scrollViewer, 0);
            grid.Children.Add(scrollViewer);

            // Zone de saisie
            var inputPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

            var inputTextBox = new TextBox
            {
                MinHeight = 30,
                VerticalContentAlignment = VerticalAlignment.Center,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap
            };
            inputTextBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("InputText") { UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged });
            inputTextBox.KeyDown += InputTextBox_KeyDown;

            var sendButton = new Button
            {
                Content = "Envoyer",
                Margin = new Thickness(5, 0, 0, 0),
                MinWidth = 80,
                IsDefault = true
            };
            sendButton.Click += SendButton_Click;

            inputPanel.Children.Add(inputTextBox);
            inputPanel.Children.Add(sendButton);

            Grid.SetRow(inputPanel, 1);
            grid.Children.Add(inputPanel);

            this.Content = grid;
        }

        private DataTemplate CreateMessageTemplate()
        {
            var template = new DataTemplate();

            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetValue(Border.MarginProperty, new Thickness(5));
            factory.SetValue(Border.PaddingProperty, new Thickness(10));
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));

            // Style conditionnel selon IsUser
            var trigger = new DataTrigger();
            trigger.Binding = new System.Windows.Data.Binding("IsUser");
            trigger.Value = true;
            trigger.Setters.Add(new Setter(Border.BackgroundProperty, System.Windows.Media.Brushes.LightBlue));
            trigger.Setters.Add(new Setter(Border.HorizontalAlignmentProperty, HorizontalAlignment.Right));

            var trigger2 = new DataTrigger();
            trigger2.Binding = new System.Windows.Data.Binding("IsUser");
            trigger2.Value = false;
            trigger2.Setters.Add(new Setter(Border.BackgroundProperty, System.Windows.Media.Brushes.LightGray));
            trigger2.Setters.Add(new Setter(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left));

            factory.Triggers.Add(trigger);
            factory.Triggers.Add(trigger2);

            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Text"));
            textBlockFactory.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            textBlockFactory.SetValue(TextBlock.MaxWidthProperty, 400.0);

            factory.AppendChild(textBlockFactory);
            template.VisualTree = factory;

            return template;
        }

        private async void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && !e.Shift)
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText) || IsLoading)
                return;

            var userMessage = InputText;
            InputText = string.Empty;

            // Ajouter le message utilisateur
            Messages.Add(new ChatMessage { IsUser = true, Text = userMessage, Timestamp = DateTime.Now });

            IsLoading = true;

            try
            {
                // Obtenir le contexte du projet actuel
                var projectContext = await GetProjectContextAsync();

                // Envoyer à l'agent middleware
                var response = await SendToAgentAsync(userMessage, projectContext);

                // Ajouter la réponse
                Messages.Add(new ChatMessage
                {
                    IsUser = false,
                    Text = response.ResponseText,
                    Timestamp = DateTime.Now
                });

                // Appliquer les modifications de fichiers si nécessaire
                if (response.ModifiedFiles?.Count > 0)
                {
                    await ApplyFileModificationsAsync(response.ModifiedFiles);
                }
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage
                {
                    IsUser = false,
                    Text = $"❌ Erreur : {ex.Message}",
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<string> GetProjectContextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await ServiceProvider.GetGlobalServiceAsync(typeof(DTE)) as DTE;
            if (dte?.Solution?.FullName == null)
                return "Aucune solution ouverte";

            var context = new StringBuilder();
            context.AppendLine($"Solution: {Path.GetFileName(dte.Solution.FullName)}");
            context.AppendLine($"Projets:");

            foreach (Project project in dte.Solution.Projects)
            {
                if (project.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") // C# project
                {
                    context.AppendLine($"- {project.Name} ({project.FileName})");
                }
            }

            // Obtenir le fichier actif si disponible
            if (dte.ActiveDocument?.FullName != null)
            {
                context.AppendLine($"Fichier actif: {dte.ActiveDocument.FullName}");

                var selection = dte.ActiveDocument.Selection as TextSelection;
                if (selection?.Text?.Length > 0)
                {
                    context.AppendLine("Code sélectionné:");
                    context.AppendLine(selection.Text);
                }
            }

            return context.ToString();
        }

        private async Task<AgentResponse> SendToAgentAsync(string message, string projectContext)
        {
            var options = AIAgentPackage.GetDialogPage(typeof(OptionsPage)) as OptionsPage;
            var apiUrl = options?.AgentApiUrl ?? "http://localhost:5000";

            var request = new AgentRequest
            {
                Message = message,
                ProjectContext = projectContext,
                Instruction = "analyze-and-respond"
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{apiUrl}/api/agent/process", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AgentResponse>(responseJson);
        }

        private async Task ApplyFileModificationsAsync(List<FileModification> modifications)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (var mod in modifications)
            {
                try
                {
                    if (File.Exists(mod.Path))
                    {
                        await File.WriteAllTextAsync(mod.Path, mod.NewContent);

                        // Recharger le fichier dans VS si ouvert
                        var dte = await ServiceProvider.GetGlobalServiceAsync(typeof(DTE)) as DTE;
                        foreach (Document doc in dte.Documents)
                        {
                            if (doc.FullName == mod.Path)
                            {
                                doc.Close(vsSaveChanges.vsSaveChangesNo);
                                dte.ItemOperations.OpenFile(mod.Path);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Messages.Add(new ChatMessage
                    {
                        IsUser = false,
                        Text = $"⚠️ Erreur lors de la modification de {mod.Path}: {ex.Message}",
                        Timestamp = DateTime.Now
                    });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Modèles de données
    public class ChatMessage
    {
        public bool IsUser { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class AgentRequest
    {
        public string Message { get; set; }
        public string ProjectContext { get; set; }
        public string Instruction { get; set; }
    }

    public class AgentResponse
    {
        public string ResponseText { get; set; }
        public List<FileModification> ModifiedFiles { get; set; }
    }

    public class FileModification
    {
        public string Path { get; set; }
        public string Diff { get; set; }
        public string NewContent { get; set; }
    }
}