using CommunityToolkit.Mvvm.ComponentModel;

namespace WinTail.Avalonia.Models
{
    /// <summary>
    /// Represents a tab with a log file.
    /// </summary>
    public partial class LogTab : ObservableObject
    {
        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private bool _isActive;

        public LogTab(string filePath)
        {
            FilePath = filePath;
            FileName = System.IO.Path.GetFileName(filePath);
        }
    }
}
