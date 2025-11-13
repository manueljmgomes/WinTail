using CommunityToolkit.Mvvm.ComponentModel;

namespace WinTail.Models
{
    /// <summary>
    /// Representa um tab com um ficheiro de log.
    /// </summary>
    public partial class LogTab : ObservableObject
    {
        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _language = "plaintext";

        [ObservableProperty]
        private bool _isActive;

        public LogTab(string filePath)
        {
            FilePath = filePath;
            FileName = System.IO.Path.GetFileName(filePath);
        }
    }
}
