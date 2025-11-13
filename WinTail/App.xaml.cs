using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using System;
using System.Linq;
using WinTail.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinTail
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private SingleInstanceService? _singleInstanceService;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Obtém os argumentos da linha de comandos
            var commandLineArgs = Environment.GetCommandLineArgs();
            var filePathArg = commandLineArgs.Length > 1 ? commandLineArgs[1] : null;

            // Inicializa o serviço de instância única
            _singleInstanceService = new SingleInstanceService();

            if (!_singleInstanceService.IsFirstInstance)
            {
                // Esta não é a primeira instância - envia o ficheiro para a instância principal
                if (!string.IsNullOrEmpty(filePathArg) && System.IO.File.Exists(filePathArg))
                {
                    _ = SingleInstanceService.SendFilePathToMainInstanceAsync(filePathArg);
                }
                
                // Termina esta instância
                Exit();
                return;
            }

            // Esta é a primeira instância - cria a janela principal
            _window = new MainWindow();
            
            // Regista o handler para receber ficheiros de outras instâncias
            _singleInstanceService.FilePathReceived += OnFilePathReceived;
            _singleInstanceService.StartListening();

            // Ativa a janela
            _window.Activate();

            // Se foi passado um ficheiro como argumento, abre-o
            if (!string.IsNullOrEmpty(filePathArg) && System.IO.File.Exists(filePathArg))
            {
                (_window as MainWindow)?.OpenLogFile(filePathArg);
            }
        }

        private void OnFilePathReceived(object? sender, string filePath)
        {
            // Executa no thread da UI
            _window?.DispatcherQueue.TryEnqueue(() =>
            {
                if (_window is MainWindow mainWindow)
                {
                    mainWindow.OpenLogFile(filePath);
                }
            });
        }
    }
}
