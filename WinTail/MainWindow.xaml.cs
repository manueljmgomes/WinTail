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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;
using WinTail.Controls;
using WinTail.Models;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinTail
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly Dictionary<string, TabViewItem> _openTabs = new();

        public MainWindow()
        {
            InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);
            
            UpdateEmptyState();
        }

        /// <summary>
        /// Abre um ficheiro de log numa nova tab ou foca a tab existente.
        /// </summary>
        public void OpenLogFile(string filePath)
        {
            // Normaliza o caminho do ficheiro
            filePath = System.IO.Path.GetFullPath(filePath);

            // Verifica se já existe uma tab para este ficheiro
            if (_openTabs.TryGetValue(filePath, out var existingTab))
            {
                LogTabView.SelectedItem = existingTab;
                Activate(); // Traz a janela para a frente
                return;
            }

            // Cria nova tab
            var logTab = new LogTab(filePath);
            var logTabView = new LogTabView(logTab);

            var tabItem = new TabViewItem
            {
                Header = logTab.FileName,
                IconSource = new SymbolIconSource { Symbol = Symbol.Document },
                Content = logTabView
            };

            _openTabs[filePath] = tabItem;
            LogTabView.TabItems.Add(tabItem);
            LogTabView.SelectedItem = tabItem;

            UpdateEmptyState();
            Activate(); // Traz a janela para a frente
        }

        private async void OpenLogButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".log");
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add("*");

            // Associa o picker com a janela
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                OpenLogFile(file.Path);
            }
        }

        private void LogTabView_AddTabButtonClick(TabView sender, object args)
        {
            OpenLogButton_Click(sender, new RoutedEventArgs());
        }

        private void LogTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Tab.Content is LogTabView logTabView)
            {
                logTabView.Cleanup();
            }

            // Remove da lista de tabs abertas
            var tabToRemove = _openTabs.FirstOrDefault(x => x.Value == args.Tab);
            if (!tabToRemove.Equals(default(KeyValuePair<string, TabViewItem>)))
            {
                _openTabs.Remove(tabToRemove.Key);
            }

            LogTabView.TabItems.Remove(args.Tab);
            UpdateEmptyState();
        }

        private void UpdateEmptyState()
        {
            EmptyStatePanel.Visibility = LogTabView.TabItems.Count == 0 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }
    }
}
